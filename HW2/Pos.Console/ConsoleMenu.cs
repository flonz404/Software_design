using System.Globalization;
using Pos.Application;
using Pos.Domain;

namespace Pos.ConsoleApp;

public sealed class ConsoleMenu
{
    private readonly AssessmentFacade _facade;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleMenu(AssessmentFacade facade, TextReader input, TextWriter output)
    {
        _facade = facade;
        _input = input;
        _output = output;
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();
            try
            {
                int choice = ReadInteger("Команда: ", 0, 11);
                if (choice == 0)
                {
                    _output.WriteLine("Работа завершена.");
                    return;
                }
                Execute(choice);
            }
            catch (EndOfStreamException)
            {
                _output.WriteLine("Ввод завершён. Незавершённая операция не выполнена.");
                return;
            }
            catch (Exception exception) when (exception is ArgumentException
                or InvalidOperationException or IOException or InvalidDataException or UnauthorizedAccessException
                or OverflowException)
            {
                _output.WriteLine($"Ошибка: {exception.Message}");
            }
        }
    }

    private void PrintMenu()
    {
        _output.WriteLine();
        _output.WriteLine("ПОС - преподавательская оценка студентов");
        _output.WriteLine("1. Добавить студента       2. Список студентов");
        _output.WriteLine("3. Добавить критерий      4. Рубрикатор");
        _output.WriteLine("5. Выставить оценку       6. Исправить оценку");
        _output.WriteLine("7. Отменить оценку        8. Оценки студента");
        _output.WriteLine("9. Итог студента         10. Общая ведомость и аналитика");
        _output.WriteLine("11. Экспорт итогов CSV    0. Выход");
    }

    private void Execute(int choice)
    {
        switch (choice)
        {
            case 1:
                int id = ReadInteger("Id студента: ", 1, int.MaxValue);
                string name = ReadRequired("Имя студента: ");
                _facade.AddStudent(id, name);
                _output.WriteLine("Студент добавлен.");
                break;
            case 2:
                ShowStudents();
                break;
            case 3:
                int criterionId = ReadInteger("Id критерия: ", 1, int.MaxValue);
                string criterionName = ReadRequired("Название: ");
                decimal maxScore = ReadDecimal("Максимальный балл (> 0): ", true);
                decimal weight = ReadDecimal("Вес (> 0): ", true);
                _facade.AddCriterion(criterionId, criterionName, maxScore, weight);
                _output.WriteLine("Критерий добавлен.");
                break;
            case 4:
                ShowCriteria();
                break;
            case 5:
            case 6:
                int studentId = ReadInteger("Id студента: ", 1, int.MaxValue);
                int evaluationCriterionId = ReadInteger("Id критерия: ", 1, int.MaxValue);
                decimal score = ReadDecimal("Балл (>= 0): ", false);
                if (choice == 5)
                {
                    _facade.AssignEvaluation(studentId, evaluationCriterionId, score);
                    _output.WriteLine("Оценка выставлена.");
                }
                else
                {
                    _facade.CorrectEvaluation(studentId, evaluationCriterionId, score);
                    _output.WriteLine("Оценка исправлена.");
                }
                break;
            case 7:
                int removeStudentId = ReadInteger("Id студента: ", 1, int.MaxValue);
                int removeCriterionId = ReadInteger("Id критерия: ", 1, int.MaxValue);
                _facade.RemoveEvaluation(removeStudentId, removeCriterionId);
                _output.WriteLine("Оценка отменена.");
                break;
            case 8:
                ShowEvaluations(ReadInteger("Id студента: ", 1, int.MaxValue));
                break;
            case 9:
                int reportStudentId = ReadInteger("Id студента: ", 1, int.MaxValue);
                PrintReport(_facade.GetStudentReport(reportStudentId, ReadMode()));
                break;
            case 10:
                ShowReport(ReadMode());
                break;
            case 11:
                CalculationMode mode = ReadMode();
                string path = Path.GetFullPath(ReadRequired("Путь к файлу .csv: "));
                if (File.Exists(path))
                {
                    int overwrite = ReadInteger("Файл существует. Перезаписать? 1 - да, 0 - нет: ", 0, 1);
                    if (overwrite == 0)
                    {
                        _output.WriteLine("Экспорт отменён.");
                        return;
                    }
                }
                _facade.Export(mode, path);
                _output.WriteLine($"CSV сохранён: {path}");
                break;
        }
    }

    private void ShowStudents()
    {
        IReadOnlyCollection<Student> students = _facade.GetStudents();
        if (students.Count == 0)
        {
            _output.WriteLine("Студентов пока нет.");
        }
        foreach (Student student in students)
        {
            _output.WriteLine($"{student.Id}: {student.Name}");
        }
    }

    private void ShowCriteria()
    {
        IReadOnlyCollection<Criterion> criteria = _facade.GetCriteria();
        if (criteria.Count == 0)
        {
            _output.WriteLine("Рубрикатор пуст.");
        }
        foreach (Criterion criterion in criteria)
        {
            _output.WriteLine($"{criterion.Id}: {criterion.Name}; максимум {criterion.MaxScore}; вес {criterion.Weight.Value}");
        }
    }

    private void ShowEvaluations(int studentId)
    {
        IReadOnlyCollection<EvaluationView> evaluations = _facade.GetEvaluations(studentId);
        if (evaluations.Count == 0)
        {
            _output.WriteLine("Рубрикатор пуст.");
        }
        foreach (EvaluationView evaluation in evaluations)
        {
            string score = "нет оценки";
            if (evaluation.Score.HasValue)
            {
                score = evaluation.Score.Value.ToString(CultureInfo.CurrentCulture);
            }
            _output.WriteLine($"{evaluation.CriterionId}: {evaluation.CriterionName}; {score} / {evaluation.MaxScore}; вес {evaluation.Weight}");
        }
    }

    public void ShowReport(CalculationMode mode)
    {
        IReadOnlyCollection<StudentReport> reports = _facade.GetReport(mode);
        if (reports.Count == 0)
        {
            _output.WriteLine("Студентов пока нет.");
            return;
        }

        decimal sum = 0;
        int calculated = 0;
        int complete = 0;
        foreach (StudentReport report in reports)
        {
            PrintReport(report);
            if (report.Result.CriterionCount > 0
                && report.Result.GradedCount == report.Result.CriterionCount)
            {
                complete++;
            }
            if (report.Result.Value.HasValue)
            {
                sum += report.Result.Value.Value;
                calculated++;
            }
        }
        _output.WriteLine($"Студентов: {reports.Count}; полностью оценены: {complete}; рассчитано: {calculated}.");
        if (calculated > 0)
        {
            _output.WriteLine($"Средний балл среди рассчитанных в выбранном режиме: {FormatScore(sum / calculated)}");
        }
    }

    private void PrintReport(StudentReport report)
    {
        string score = "нет итога";
        if (report.Result.Value.HasValue)
        {
            score = FormatScore(report.Result.Value.Value);
        }
        _output.WriteLine($"{report.StudentId}: {report.StudentName} | {score} | {report.Result.Message} | оценок {report.Result.GradedCount}/{report.Result.CriterionCount}");
    }

    private static string FormatScore(decimal score)
    {
        return decimal.Round(score, 3, MidpointRounding.AwayFromZero)
            .ToString("0.###", CultureInfo.CurrentCulture);
    }

    private CalculationMode ReadMode()
    {
        return (CalculationMode)ReadInteger("Режим: 1 - окончательный, 2 - предварительный: ", 1, 2);
    }

    private string ReadRequired(string prompt)
    {
        while (true)
        {
            _output.Write(prompt);
            string? value = _input.ReadLine();
            if (value is null)
            {
                throw new EndOfStreamException();
            }
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
            _output.WriteLine("Введите непустое значение.");
        }
    }

    private int ReadInteger(string prompt, int min, int max)
    {
        while (true)
        {
            string text = ReadRequired(prompt);
            if (int.TryParse(text, out int value) && value >= min && value <= max)
            {
                return value;
            }
            _output.WriteLine($"Введите целое число от {min} до {max}.");
        }
    }

    private decimal ReadDecimal(string prompt, bool positive)
    {
        while (true)
        {
            string text = ReadRequired(prompt).Replace(',', '.');
            NumberStyles styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
            if (decimal.TryParse(text, styles, CultureInfo.InvariantCulture, out decimal value)
                && (positive ? value > 0 : value >= 0))
            {
                return value;
            }
            _output.WriteLine("Введите допустимое число. Разделитель дробной части: точка или запятая.");
        }
    }
}
