using System.Text;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using LessonFlow.Extensionss;
using Tabula;
using Tabula.Detectors;
using Tabula.Extractors;
using UglyToad.PdfPig;

namespace LessonFlow.Api.Services.CurriculumParser.SACurriculum;

public abstract class BaseParser(string subjectName, char[] charsToRemove)
{
    protected readonly char[] _charsToRemove = charsToRemove;
    protected readonly string _subjectName = subjectName;
    protected int _currentPageNum = 2;
    protected PageType _currentPageType = PageType.LearningStandard;

    public Subject ParseFile(string file)
    {
        var yearLevels = new List<YearLevel>();
        var description = string.Empty;

        using var document = PdfDocument.Open(file);
        while (_currentPageNum < document.NumberOfPages)
        {
            var yearLevel = ParseLearningStandard(document, ref description);
            ParseDispositionsAndCapabilities(document, yearLevel);
            var conceptualOrganisers = ParseConceptualOrganisers(document);

            yearLevel.SetConceptualOrganisers(conceptualOrganisers);
            RemoveUnusedConceptualOrganisers(yearLevel);
            yearLevels.Add(yearLevel);
        }

        return new Subject(_subjectName, yearLevels, description);
    }

    protected virtual YearLevel ParseLearningStandard(PdfDocument document, ref string description)
    {
        var page = document.GetPage(_currentPageNum);
        var descriptionBuilder = new StringBuilder();
        var words = page.Text.Split(' ');
        var idx = 0;

        // Get name of the subject as written in the curriculum
        do
        {
            var word = words[idx];
            if (word != _subjectName)
            {
                idx++;
                continue;
            }

            idx++;
            word = words[idx];
            if (word == "Reception")
            {
                // Handles the first occurrence of the subject name which is in the footer of the page
                continue;
            }

            break;
        } while (idx < words.Length);

        // Get the subject description to pass up to the subject
        do
        {
            idx++;
            if (words[idx] == "Learning")
            {
                idx++;
                if (words[idx] == "Standard")
                {
                    break;
                }

                descriptionBuilder.Append("Learning");
            }

            if (words[idx] == string.Empty)
            {
                descriptionBuilder.Append("\n\n");
            }
            else
            {
                descriptionBuilder.Append(words[idx]);
                descriptionBuilder.Append(' ');
            }
        } while (idx < words.Length);

        YearLevelValue yearLevelValue;
        var desc = descriptionBuilder.ToString();
        var wordIdx = desc.Length - 7; // The shortest YearLevelValue is 6 so start here.
        do
        {
            if (wordIdx == 0)
            {
                throw new Exception("Year Level not found");
            }

            try
            {
                yearLevelValue = Enum.Parse<YearLevelValue>(desc[wordIdx..].Replace(" ", string.Empty));
                break;
            }
            catch
            {
                wordIdx--;
            }
        } while (true);

        description = desc[..wordIdx].Trim();

        var learningStandardBuilder = new StringBuilder();
        idx++;
        var prev = words[idx]; // to handle multiple consecutive empty strings
        do
        {
            if (words[idx] == "UNDERSTAND:")
            {
                break;
            }

            if (words[idx] == string.Empty)
            {
                if (prev != string.Empty)
                {
                    learningStandardBuilder.Append("\n\n");
                    prev = words[idx];
                }

                idx++;
                continue;
            }

            learningStandardBuilder.Append(words[idx]);
            learningStandardBuilder.Append(' ');
            prev = words[idx];
            idx++;
        } while (idx < words.Length);

        var learningStandard = learningStandardBuilder.ToString().Trim();
        _currentPageNum++;

        return new YearLevel(yearLevelValue, learningStandard);
    }

    protected virtual void ParseDispositionsAndCapabilities(PdfDocument document, YearLevel yearLevel)
    {
        var extractor = new ObjectExtractor(document);
        var pageArea = extractor.Extract(_currentPageNum);
        var detector = new SimpleNurminenDetectionAlgorithm();
        var regions = detector.Detect(pageArea);
        var ea = new BasicExtractionAlgorithm();

        var dispositions = ParseDispositions(ea.Extract(pageArea.GetArea(regions[1].BoundingBox))[0]);
        var capabilities = ParseCapabilities(ea.Extract(pageArea.GetArea(regions[0].BoundingBox))[0]);

        _currentPageType = PageType.Dispositions;
        yearLevel.SetDispositions(dispositions);
        yearLevel.SetCapabilities(capabilities);
        _currentPageNum++;
    }

    protected virtual List<Disposition> ParseDispositions(Table table)
    {
        var dispositions = new List<Disposition>();
        var rows = table.Rows;
        foreach (var cell in rows[0])
        {
            dispositions.Add(new Disposition { Title = cell.GetText() });
        }

        for (var i = 2; i < table.Rows.Count; i++)
        {
            // skip the second row's boilerplate text
            for (var j = 0; j < table.ColumnCount; j++)
            {
                var text = rows[i][j].GetText();

                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                if (text.EndsWith("for example:"))
                {
                    continue;
                }

                if (text.StartsWith('•'))
                {
                    var textStart = 1;
                    for (var k = textStart; k < text.Length; k++)
                    {
                        if (!char.IsWhiteSpace(text[k]))
                        {
                            textStart = k;
                            break;
                        }
                    }

                    dispositions[j].DevelopedWhen.Add(text[textStart..].CapitaliseFirstLetter());
                }
                else
                {
                    dispositions[j].DevelopedWhen[^1] += " " + text;
                }
            }
        }

        return dispositions;
    }

    protected virtual List<Capability> ParseCapabilities(Table table)
    {
        var capabilities = new List<Capability>();
        var rows = table.Rows;
        foreach (var cell in rows[0])
        {
            capabilities.Add(new Capability { Name = cell.GetText() });
        }

        // find the starting row for each disposition column to avoid the boilerplate text overrunning the line
        var rowStarts = new int[table.ColumnCount];
        for (var i = 1; i < table.Rows.Count; i++)
        {
            for (var j = 0; j < table.ColumnCount; j++)
            {
                if (rows[i][j].GetText().EndsWith(':'))
                {
                    // the start of each disposition ends with "for example:"
                    if (rowStarts[j] == 0)
                    {
                        // takes only the first value to account for situations where one of the capabilities also ends with ':'
                        rowStarts[j] = i + 1;
                    }
                }
            }
        }

        var startIdx = rowStarts.Min();
        for (var i = startIdx; i < table.Rows.Count; i++)
        {
            // skip the second row's boilerplate text
            for (var j = 0; j < table.ColumnCount; j++)
            {
                if (i < rowStarts[j])
                {
                    continue;
                }

                var text = rows[i][j].GetText().Trim();
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                if (text.StartsWith('•'))
                {
                    var textStart = 1;
                    for (var k = textStart; k < text.Length; k++)
                    {
                        if (!char.IsWhiteSpace(text[k]))
                        {
                            textStart = k;
                            break;
                        }
                    }

                    capabilities[j].Descriptors.Add(text[textStart..].CapitaliseFirstLetter());
                }
                else
                {
                    if (text.StartsWith('օ'))
                    {
                        capabilities[j].Descriptors[^1] += "\n" + text[1..];
                    }
                    else if (text.EndsWith('-'))
                    {
                        capabilities[j].Descriptors[^1] += text;
                    }
                    else
                    {
                        capabilities[j].Descriptors[^1] += " " + text;
                    }
                }
            }
        }

        return capabilities;
    }

    protected virtual List<ConceptualOrganiser> ParseConceptualOrganisers(PdfDocument document)
    {
        var extractor = new ObjectExtractor(document);
        var detector = new SimpleNurminenDetectionAlgorithm();
        var ea = new BasicExtractionAlgorithm();
        PageArea pageArea;
        List<TableRectangle> regions;
        Table table;

        var conceptualOrganisers = new List<ConceptualOrganiser>();

        while (_currentPageNum < document.NumberOfPages && _currentPageType != PageType.LearningStandard)
        {
            pageArea = extractor.Extract(_currentPageNum);
            regions = detector.Detect(pageArea);
            table = ea.Extract(pageArea.GetArea(regions[0].BoundingBox))[0];

            _currentPageType = DeterminePageType(document, _currentPageType);
            if (_currentPageType == PageType.LearningStandard)
            {
                break;
            }

            if (_currentPageType == PageType.AdditionalContentDescriptions)
            {
                var algo = new SpreadsheetExtractionAlgorithm();
                table = algo.Extract(pageArea)[0];
            }

            switch (_currentPageType)
            {
                case PageType.KnowledgeWithCDs:
                    ParseKnowledgeWithContentDescriptionsTable(table, conceptualOrganisers);
                    break;
                case PageType.KnowledgeWithoutCDs:
                    ParseKnowledgeWithoutContentDescriptionsTable(table, conceptualOrganisers);
                    break;
                case PageType.ContentDescriptionsOnly:
                    var idx = 1;
                    ParseContentDescriptions(table, conceptualOrganisers, ref idx);
                    break;
                case PageType.AdditionalContentDescriptions:
                    ParseAdditionalContentDescriptionsTable(table, conceptualOrganisers);
                    break;
                default:
                    throw new ArgumentException($"Unexpected page type: {_currentPageType}");
            }

            _currentPageNum++;
        }

        return conceptualOrganisers;
    }

    private void ParseKnowledgeWithContentDescriptionsTable(Table table, List<ConceptualOrganiser> conceptualOrganisers)
    {
        var rows = table.Rows;
        foreach (var cell in rows[0])
        {
            conceptualOrganisers.Add(new ConceptualOrganiser { Name = cell.GetText() });
        }

        var rowIdx = 1;
        // find the starting row for each "Why it matters" section column as they are all different length
        var rowStarts = new int[table.ColumnCount];
        do
        {
            // What it is 
            for (var i = 0; i < table.ColumnCount; i++)
            {
                var text = rows[rowIdx][i].GetText();
                if (text.StartsWith("What it is"))
                {
                    conceptualOrganisers[i].WhatItIs = text;
                }
                else if (text.StartsWith("Why it matters"))
                {
                    rowStarts[i] = rowIdx;
                }
                else if (rowStarts[i] != 0)
                {
                }
                else
                {
                    conceptualOrganisers[i].WhatItIs += " " + text;
                }
            }

            rowIdx++;
        } while (rowStarts.Min() == 0);

        rowIdx = rowStarts.Min();
        do
        {
            // Why it matters
            for (var i = 0; i < table.ColumnCount; i++)
            {
                if (rowIdx < rowStarts[i])
                {
                    continue;
                }

                var text = rows[rowIdx][i].GetText();
                if (text.StartsWith("Why it matters"))
                {
                    conceptualOrganisers[i].WhyItMatters = text;
                }
                else
                {
                    conceptualOrganisers[i].WhyItMatters += " " + text;
                }
            }

            rowIdx++;
        } while (!rows[rowIdx][0].GetText().StartsWith("Conceptual understandings"));

        rowIdx++; // skip conceptual understandings row
        ParseConceptualUnderstandings(table, conceptualOrganisers, ref rows, ref rowIdx);

        rowIdx++; // skip supporting content descriptions row
        ParseContentDescriptions(table, conceptualOrganisers, ref rowIdx);
    }

    private void ParseConceptualUnderstandings(Table table, List<ConceptualOrganiser> conceptualOrganisers,
        ref IReadOnlyList<IReadOnlyList<Cell>> rows, ref int rowIdx)
    {
        var startNewUnderstanding = new bool[table.ColumnCount];
        Array.Fill(startNewUnderstanding, true);
        var startIdx = rowIdx;

        while (!rows[rowIdx][0].GetText().StartsWith("Supporting content descriptions"))
        {
            rowIdx++;
        }

        // Get the indices in the columns where the new conceptual understandings start.
        var newConceptualUnderstandingsIndices = new List<int>[table.ColumnCount];
        for (var i = 0; i < table.ColumnCount; i++)
        {
            var wordString = string.Empty;
            newConceptualUnderstandingsIndices[i] = [];
            for (var j = startIdx; j < rowIdx; j++)
            {
                wordString += rows[j][i].GetText();
            }

            for (var k = 0; k < wordString.Length - 1; k++)
            {
                if (wordString[k] == '.' && char.IsUpper(wordString[k + 1]))
                {
                    newConceptualUnderstandingsIndices[i].Add(k + 1);
                }
            }
        }

        for (var i = 0; i < table.ColumnCount; i++)
        {
            var understandings = new List<string>();
            var createNewUnderstanding = true;
            var currentIdx = 0;
            var totalLetters = 0;
            for (var j = startIdx; j < rowIdx; j++)
            {
                var text = rows[j][i].GetText();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (currentIdx == newConceptualUnderstandingsIndices[i].Count)
                {
                    AddConceptualUnderstanding(understandings, text);
                }
                else if (text.Length + totalLetters < newConceptualUnderstandingsIndices[i][currentIdx])
                {
                    if (createNewUnderstanding)
                    {
                        understandings.Add(text);
                        createNewUnderstanding = false;
                    }
                    else
                    {
                        understandings[^1] += " " + text;
                    }
                }
                else if (text.Length + totalLetters > newConceptualUnderstandingsIndices[i][currentIdx])
                {
                    var idx = newConceptualUnderstandingsIndices[i][currentIdx] - totalLetters;
                    if (idx > 0)
                    {
                        understandings[^1] += " " + text[..idx];
                    }

                    understandings.Add(text[idx..]);
                    currentIdx++;
                    createNewUnderstanding = false;
                }
                else
                {
                    AddConceptualUnderstanding(understandings, text);
                    createNewUnderstanding = true;
                }

                totalLetters += text.Length;
            }

            conceptualOrganisers[i].ConceptualUnderstandings = understandings;
        }
    }

    private void AddConceptualUnderstanding(List<string> understandings, string text)
    {
        if (understandings.Count == 0)
        {
            understandings.Add(text);
        }
        else
        {
            understandings[^1] += " " + text;
        }
    }

    private void ParseContentDescriptions(Table table, List<ConceptualOrganiser> conceptualOrganisers, ref int rowIdx)
    {
        var contentDescriptionCounts = new int[table.ColumnCount];
        var rows = table.Rows;

        do
        {
            // supporting content descriptions
            for (var i = 0; i < table.ColumnCount; i++)
            {
                var text = rows[rowIdx][i].GetText();
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                if (IsOnlyCurriculumCodes(text) && CurriculumCodesAreAlreadyCaptured(conceptualOrganisers[i], text))
                {
                    continue;
                }

                if (IsContentDescriptionEnd(text))
                {
                    var curriculumCodeStart = FindCurriculumCodeIdx(text);
                    if (text.Contains('\r'))
                    {
                        text = text.Replace('\r', ' ');
                    }


                    var curriculumCodes = text[curriculumCodeStart..].Length > 0
                        ? text[curriculumCodeStart..].Split(' ')
                        : [];
                    var descriptionText = text[..curriculumCodeStart];
                    if (rowIdx + 1 < rows.Count && rows[rowIdx + 1][i].GetText().StartsWith("AC9"))
                    {
                        var extraCurriculumCodes = rows[rowIdx + 1][i].GetText().Split(' ');
                        curriculumCodes = [.. curriculumCodes, .. extraCurriculumCodes];
                    }

                    AddContentDescriptionText(conceptualOrganisers[i], descriptionText, contentDescriptionCounts[i]);
                    if (curriculumCodes.Length > 0)
                    {
                        conceptualOrganisers[i].ContentDescriptions[contentDescriptionCounts[i]].CurriculumCodes =
                            curriculumCodes;
                    }

                    contentDescriptionCounts[i]++;
                }
                else
                {
                    AddContentDescriptionText(conceptualOrganisers[i], text, contentDescriptionCounts[i]);
                }
            }
        } while (++rowIdx < rows.Count);
    }

    private static bool CurriculumCodesAreAlreadyCaptured(ConceptualOrganiser conceptualOrganiser, string text)
    {
        if (conceptualOrganiser.ContentDescriptions[^1].CurriculumCodes is null)
        {
            return false;
        }

        var codes = text.Split(' ');
        foreach (var code in codes)
        {
            if (!conceptualOrganiser.ContentDescriptions[^1].CurriculumCodes!.Contains(code))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsOnlyCurriculumCodes(string text)
    {
        return text.Split(' ').All(t => t.StartsWith("AC9"));
    }

    private void ParseKnowledgeWithoutContentDescriptionsTable(Table table,
        List<ConceptualOrganiser> conceptualOrganisers)
    {
        var rows = table.Rows;
        foreach (var cell in rows[0])
        {
            conceptualOrganisers.Add(new ConceptualOrganiser { Name = cell.GetText() });
        }

        var rowIdx = 1;
        // find the starting row for each "Why it matters" section column as they are all different length
        var rowStarts = new int[table.ColumnCount];
        do
        {
            // What it is 
            for (var i = 0; i < table.ColumnCount; i++)
            {
                var text = rows[rowIdx][i].GetText();
                if (text.StartsWith("What it is"))
                {
                    conceptualOrganisers[i].WhatItIs = text;
                }
                else if (text.StartsWith("Why it matters"))
                {
                    rowStarts[i] = rowIdx;
                }
                else
                {
                    conceptualOrganisers[i].WhatItIs += " " + text;
                }
            }

            rowIdx++;
        } while (rowStarts.Min() == 0);

        rowIdx = rowStarts.Min();
        do
        {
            // Why it matters
            for (var i = 0; i < table.ColumnCount; i++)
            {
                if (rowIdx < rowStarts[i])
                {
                    continue;
                }

                var text = rows[rowIdx][i].GetText();
                if (text.StartsWith("Why it matters"))
                {
                    conceptualOrganisers[i].WhyItMatters = text;
                }
                else
                {
                    conceptualOrganisers[i].WhyItMatters += " " + text;
                }
            }

            rowIdx++;
        } while (!rows[rowIdx][0].GetText().StartsWith("Conceptual understandings"));

        rowIdx++; // skip conceptual understandings row

        do
        {
            // Conceptual understandings
            for (var i = 0; i < table.ColumnCount; i++)
            {
                var text = rows[rowIdx][i].GetText();
                if (text.EndsWith('.'))
                {
                    conceptualOrganisers[i].ConceptualUnderstandings[^1] += text;
                }
                else
                {
                    conceptualOrganisers[i].ConceptualUnderstandings.Add(text + " ");
                }
            }

            rowIdx++;
        } while (rowIdx < rows.Count);
    }

    private void ParseAdditionalContentDescriptionsTable(Table table, List<ConceptualOrganiser> conceptualOrganisers)
    {
        var rows = table.Rows;
        var contentDescriptionCounts = conceptualOrganisers.Select(c => c.ContentDescriptions.Count).ToArray();

        // a content description will be partial if it has no curriculumCodes or has curriculumCodes but the next row starts with a curriculum code
        for (var i = 0; i < table.ColumnCount; i++)
        {
            if (conceptualOrganisers[i].ContentDescriptions.Count == 0)
            {
                continue;
            }

            if (conceptualOrganisers[i].ContentDescriptions[contentDescriptionCounts[i] - 1].CurriculumCodes == null)
            {
                contentDescriptionCounts[i]--;
            }
            else if (rows[0][i].GetText().StartsWith("AC9"))
            {
                contentDescriptionCounts[i]--;
            }
        }

        for (var i = 0; i < table.ColumnCount; i++)
        {
            // the spreadsheet algorithm that we are using for this function specifically generates a single row with all the text in one. 
            var content = rows[0][i].GetText();
            if (content == string.Empty)
            {
                continue;
            }

            var words = content.Replace('\r', ' ').Split(' ');

            for (var j = 0; j < words.Length; j++)
            {
                var text = "";
                var curriculumCodes = new List<string>();
                while (j < words.Length && !words[j].StartsWith("AC9"))
                {
                    text += words[j] + ' ';
                    j++;
                }

                while (j < words.Length && words[j].StartsWith("AC9"))
                {
                    curriculumCodes.Add(words[j]);
                    j++;
                }

                AddContentDescriptionText(conceptualOrganisers[i], text, contentDescriptionCounts[i]);

                conceptualOrganisers[i].ContentDescriptions[contentDescriptionCounts[i]].CurriculumCodes =
                    [.. curriculumCodes];
                contentDescriptionCounts[i]++;
            }
        }
    }

    protected virtual bool IsContentDescriptionEnd(string text)
    {
        return text.Contains("AC9");
    }

    private static int FindCurriculumCodeIdx(string text)
    {
        for (var i = 0; i < text.Length - 1; i++)
        {
            if (text[i] == 'A' && text[i + 1] == 'C' && text[i + 2] == '9')
            {
                return i;
            }
        }

        return
            text.Length; // addresses situations where the text has no curriculumCode but the string ends with '^' or similar 
    }

    private void AddContentDescriptionText(ConceptualOrganiser co, string text, int idx)
    {
        foreach (var c in _charsToRemove)
        {
            if (text.IndexOf(c) > 0)
            {
                text = text.Remove(text.IndexOf(c));
            }
        }

        text = text.Trim();

        if (co.ContentDescriptions.Count <= idx)
        {
            co.ContentDescriptions.Add(new ContentDescription
            {
                ConceptualOrganiser = co,
                Text = text.CapitaliseFirstLetter()
            });
        }
        else
        {
            co.ContentDescriptions[idx].Text += " " + text;
        }
    }

    private PageType DeterminePageType(PdfDocument document, PageType currentPageType)
    {
        var hasConceptualUnderstandings = false;
        var hasSupportingContentDescriptionsTitle = false;
        var text = string.Join(' ', document.GetPage(_currentPageNum).GetWords());

        if (text.Contains("Learning Standard"))
        {
            return PageType.LearningStandard;
        }

        if (text.Contains("This disposition"))
        {
            return PageType.Dispositions;
        }

        if (text.Contains("Conceptual understandings"))
        {
            hasConceptualUnderstandings = true;
        }

        if (text.Contains("Supporting content descriptions"))
        {
            hasSupportingContentDescriptionsTitle = true;
        }

        return (hasConceptualUnderstandings, hasSupportingContentDescriptionsTitle, currentPageType) switch
        {
            (true, true, _) => PageType.KnowledgeWithCDs,
            (true, false, _) => PageType.KnowledgeWithoutCDs,
            (false, true, _) => PageType.ContentDescriptionsOnly,
            (false, false, PageType.AdditionalContentDescriptions) => PageType.LearningStandard,
            (false, false, _) => PageType.AdditionalContentDescriptions
        };
    }

    private static void RemoveUnusedConceptualOrganisers(YearLevel yearLevel)
    {
        var conceptualOrganisers = new List<ConceptualOrganiser>();
        foreach (var conceptualOrganiser in yearLevel.ConceptualOrganisers)
        {
            if (conceptualOrganiser.ContentDescriptions.Count > 0)
            {
                conceptualOrganisers.Add(conceptualOrganiser);
            }
        }

        yearLevel.SetConceptualOrganisers(conceptualOrganisers);
    }
}