using LessonFlow.Domain.Curriculum;
using Tabula;
using Tabula.Detectors;
using Tabula.Extractors;
using UglyToad.PdfPig;

namespace LessonFlow.Services.CurriculumParser.SACurriculum;

public class EnglishParser() : BaseParser("English", _contentDescriptionEndings)
{
    private static readonly char[] _contentDescriptionEndings = ['*'];

    protected override void ParseDispositionsAndCapabilities(PdfDocument document, YearLevel yearLevel)
    {
        var pageArea = ObjectExtractor.Extract(document, _currentPageNum);
        var detector = new SimpleNurminenDetectionAlgorithm();
        var regions = detector.Detect(pageArea);
        var ea = new BasicExtractionAlgorithm();

        Table[] tables =
        [
            ea.Extract(pageArea.GetArea(regions[0].BoundingBox))[0],
            ea.Extract(pageArea.GetArea(regions[1].BoundingBox))[0]
        ];
        var dispositionsIdx = DetermineDispositionsTable(tables);
        var dispositions = ParseDispositions(tables[dispositionsIdx]);
        var capabilitiesIdx = dispositionsIdx == 0 ? 1 : 0;
        var capabilities = ParseCapabilities(tables[capabilitiesIdx]);

        _currentPageType = PageType.Dispositions;
        yearLevel.SetDispositions(dispositions);
        yearLevel.SetCapabilities(capabilities);
        _currentPageNum++;
    }

    private static int DetermineDispositionsTable(Table[] tables)
    {
        foreach (var row in tables[0].Rows)
        {
            foreach (var cell in row)
            {
                if (cell.GetText().Contains("disposition"))
                {
                    return 0;
                }
            }
        }

        return 1;
    }
}