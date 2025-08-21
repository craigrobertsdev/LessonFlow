using LessonFlow.Domain.Curriculum;
using LessonFlow.Interfaces.Curriculum;

namespace LessonFlow.Api.Services.CurriculumParser.SACurriculum;

public class SACurriculumParser : ICurriculumParser
{
    public async Task<List<Subject>> ParseCurriculum()
    {
        var curriculum = new List<Subject>();

        // var subjectDirectory = @"C:\Users\craig\source\repos\LessonFlow.Api\src\LessonFlow.Api\CurriculumFiles";
        var subjectDirectory = @"/home/craig/repos/LessonFlow/src/LessonFlow.Api/CurriculumFiles";
        var files = Directory.GetFiles(subjectDirectory, "*.pdf");

        List<Task<Subject>> tasks = [];

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (fileName.StartsWith("The_Arts"))
            {
                var subjectName = fileName.Split('-')[1][..^4]; // remove the "The_Arts-" and ".pdf" extension
                tasks.Add(Task.Run(() => new ArtsParser(subjectName).ParseFile(file)));
            }
            else if (fileName.Equals("English.pdf"))
            {
                tasks.Add(Task.Run(() => new EnglishParser().ParseFile(file)));
            }
            else if (fileName.Equals("Mathematics.pdf"))
            {
                tasks.Add(Task.Run(() => new MathematicsParser().ParseFile(file)));
            }
            else if (fileName.StartsWith("Language"))
            {
                var subjectName = fileName.Split('-')[1][..^4]; // remove the "Language-" and ".pdf" extension
                tasks.Add(Task.Run(() => new LanguageParser(subjectName).ParseFile(file)));
            }
            else
            {
                throw new NotSupportedException(file);
            }

            var subjects = await Task.WhenAll(tasks);

            curriculum.AddRange(subjects);
        }

        return curriculum;
    }
}