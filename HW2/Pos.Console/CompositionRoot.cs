using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.ConsoleApp;

public static class CompositionRoot
{
    public static AssessmentFacade CreateFacade(IGradebookRepository repository)
    {
        SimpleContainer container = new SimpleContainer();
        container.Register<IGradebookRepository>(CreateRepository);
        container.Register<IReportExporter>(CreateExporter);
        container.Register<FinalScoreCalculator>(CreateCalculator);
        container.Register<StrictScoringStrategy>(CreateStrictStrategy);
        container.Register<PreliminaryScoringStrategy>(CreatePreliminaryStrategy);
        container.Register<AssessmentService>(CreateAssessmentService);
        container.Register<ReportService>(CreateReportService);
        container.Register<AssessmentFacade>(CreateAssessmentFacade);
        return container.Resolve<AssessmentFacade>();

        IGradebookRepository CreateRepository(SimpleContainer services)
        {
            return repository;
        }
    }

    private static IReportExporter CreateExporter(SimpleContainer services)
    {
        return new CsvReportExporter();
    }

    private static FinalScoreCalculator CreateCalculator(SimpleContainer services)
    {
        return new FinalScoreCalculator();
    }

    private static StrictScoringStrategy CreateStrictStrategy(SimpleContainer services)
    {
        return new StrictScoringStrategy();
    }

    private static PreliminaryScoringStrategy CreatePreliminaryStrategy(SimpleContainer services)
    {
        return new PreliminaryScoringStrategy();
    }

    private static AssessmentService CreateAssessmentService(SimpleContainer services)
    {
        return new AssessmentService(services.Resolve<IGradebookRepository>());
    }

    private static ReportService CreateReportService(SimpleContainer services)
    {
        return new ReportService(services.Resolve<IGradebookRepository>(),
            services.Resolve<FinalScoreCalculator>(), services.Resolve<StrictScoringStrategy>(),
            services.Resolve<PreliminaryScoringStrategy>(), services.Resolve<IReportExporter>());
    }

    private static AssessmentFacade CreateAssessmentFacade(SimpleContainer services)
    {
        return new AssessmentFacade(services.Resolve<AssessmentService>(),
            services.Resolve<ReportService>());
    }
}
