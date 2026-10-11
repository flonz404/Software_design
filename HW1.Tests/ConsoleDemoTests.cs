using System.Globalization;
using System.Reflection;
using VyshKat.Services;
using Xunit;

namespace VyshKat.Tests;

// Console.Out общий для процесса, поэтому этот тест выполняется отдельно от остальных.
[CollectionDefinition("Console demonstration", DisableParallelization = true)]
public class ConsoleDemoCollection
{
}

[Collection("Console demonstration")]
public class ConsoleDemoTests
{
    [Fact]
    public void Main_WhenDemoContainsInvalidValues_ContinuesToFinalReport()
    {
        // Arrange: получаем точку входа собранного консольного приложения.
        MethodInfo? entryPoint = typeof(InventoryService).Assembly.EntryPoint;
        Assert.NotNull(entryPoint);
        TextWriter originalOutput = Console.Out;
        using StringWriter capturedOutput = new StringWriter();

        // Act: перехватываем вывод; finally восстановит его даже при ошибке.
        try
        {
            Console.SetOut(capturedOutput);
            entryPoint.Invoke(null, new object[] { Array.Empty<string>() });
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        // Assert: ошибки показаны, выполнение дошло до отчёта без потери инвентаря.
        string transcript = capturedOutput.ToString();
        Assert.Contains("Инвентарь пуст.", transcript);
        Assert.Contains("Удобство 0:", transcript);
        Assert.Contains("Удобство 11:", transcript);
        Assert.Contains("Потребление -1:", transcript);

        int finalReportStart = transcript.LastIndexOf(
            "6. Инвентарь после отклонения некорректных данных",
            StringComparison.Ordinal);
        Assert.True(finalReportStart >= 0, "Итоговый отчёт не был выведен.");
        string finalReport = transcript.Substring(finalReportStart);
        string expectedEnergy = 3.5.ToString(CultureInfo.CurrentCulture);

        Assert.Contains("Предметов в инвентаре: 6", finalReport);
        Assert.Contains($"Потребление за сутки: {expectedEnergy} кВт·ч", finalReport);
        Assert.DoesNotContain("№8:", finalReport);
    }
}
