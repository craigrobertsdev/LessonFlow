using System.Text;
using LessonFlow.Domain.Curriculum;
using LessonFlow.Domain.Enums;
using UglyToad.PdfPig;

namespace LessonFlow.Services.CurriculumParser.SACurriculum;

public class ArtsParser(string subjectName) : BaseParser(subjectName, _contentDescriptionEndings)
{
    private static readonly char[] _contentDescriptionEndings = ['^'];

    protected override YearLevel ParseLearningStandard(PdfDocument document, ref string description)
    {
        var page = document.GetPage(_currentPageNum);
        var descriptionBuilder = new StringBuilder();
        var words = page.Text.Split(' ');
        var idx = 0;

        var found = false;
        do
        {
            var word = words[idx];
            if (word != "The")
            {
                idx++;
                continue;
            }

            idx++;
            word = words[idx];
            if (word == "Arts" && !found)
            {
                // Handles the first occurrence of the subject name which is in the footer of the page
                found = true;
                continue;
            }

            break;
        } while (++idx < words.Length);

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
                if ((_subjectName == "Media" || _subjectName == "Music") && _currentPageNum == 14)
                {
                    yearLevelValue = YearLevelValue.Years7To8;
                    wordIdx = 361;
                }
                else if (_subjectName == "Visual Arts" && _currentPageNum == 14)
                {
                    yearLevelValue = YearLevelValue.Years7To8;
                    wordIdx = 356;
                }

                else
                {
                    var formattedStr = desc[wordIdx..].Replace("to", "To").Replace(" ", string.Empty);
                    yearLevelValue = Enum.Parse<YearLevelValue>(formattedStr);
                }

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
            var word = words[idx];
            if (word == "UNDERSTAND:")
            {
                break;
            }

            if (word == "Students") // the documents follow a consistent pattern at the start of every paragraph
            {
                learningStandardBuilder.Append("\n\n");
            }

            learningStandardBuilder.Append(word);
            learningStandardBuilder.Append(' ');
            prev = word;
            idx++;
        } while (idx < words.Length);

        var learningStandard = learningStandardBuilder.ToString().Trim();
        _currentPageNum++;

        return new YearLevel(yearLevelValue, learningStandard);
    }
}