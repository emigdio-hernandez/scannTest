using System.Globalization;
using System.Reflection;
using Abax2InlineXBRLGenerator.Generator;
using Abax2InlineXBRLGenerator.Model;
using Abax2InlineXBRLGenerator.Util;
using AbaxXBRLCnbvPersistence.Constants;
using AbaxXBRLCnbvPersistence.Persistence.Entity.Contexts;
using AbaxXBRLCnbvPersistence.Persistence.Repositoy.Impl;
using AbaxXBRLCnbvPersistence.Services.Impl;
using AbaxXBRLCore.Common.Constants;
using AbaxXBRLCore.Persistence.Entity.Contexts;
using AbaxXBRLRealTime.Model.JBRL;
using AbaxXBRLRealTime.Services;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;


namespace TestAbax2InlineXBRLGenerator.Base;

/// <summary>
/// Base class for all tests that generate IXBRL documents
/// </summary>
public class GenerateIXbrlBaseTest
{
    protected FilingProcessService FilingProcessService { get; set; }
    protected AbaxXBRLReports.Providers.Generic.Impl.MainTaxonomyInformationProvider TaxonomyResolutionService { get; set; }

    /// <summary>
    /// Initializes the common components and services required for generating IXBRL documents
    /// </summary>
    protected void init()
    {
        var EnvironmentVariables =
            JsonConvert.DeserializeObject<Dictionary<string, string>>(
                File.ReadAllText("../../../Resources/local-settings.production-migration.json"));
        foreach (var envVar in EnvironmentVariables)
        {
            Environment.SetEnvironmentVariable(envVar.Key, envVar.Value);
        }

        var Assemblies = GetProvidersAssemblies();
        TaxonomyResolutionService =
            new AbaxXBRLReports.Providers.Generic.Impl.MainTaxonomyInformationProvider(Assemblies);
        var XbrlReportServiceProvider = new AbaxXBRLReports.Providers.Generic.Impl.MainXBRLReportProvider(Assemblies);
        InitializeMongoSerializers();

        var Abax2DbContext = new AbaxDBContext();
        var CompanyRepository = new AbaxXBRLCore.Persistence.Repository.Implementation.EntityRepository(Abax2DbContext);
        var EditorInformationWorkspaceRepository =
            new AbaxXBRLCore.Persistence.Repository.Implementation.EditorInformationWorkspaceRepository(Abax2DbContext);
        var XbrlQueryService = new JBRLInstanceDocumentQueryService();
        var JbrlDocumentService =
            new JBRLInstanceDocumentService(TaxonomyResolutionService, XbrlQueryService, null, CompanyRepository);

        var AbaxRecDbContext = new AbaxRecDBContext();
        var FilingRepository = new CnbvXbrlReportFilingRepository(AbaxRecDbContext);
        var FilingBlobRepository = new CnbvXbrlReportFilingBlobRepository(AbaxRecDbContext);
        var TaxonomyRepository = new XbrlTaxonomyRegRepository(AbaxRecDbContext);
        var taxonomyRoleRepository = new XbrlTaxonomyRoleRepository(AbaxRecDbContext);
        var creationParamRepository = new CnbvXbrlReportFilingCreationParamRepository(AbaxRecDbContext);

        FilingProcessService = new FilingProcessService(
            FilingRepository,
            FilingBlobRepository,
            TaxonomyRepository,
            taxonomyRoleRepository,
            creationParamRepository,
            JbrlDocumentService,
            XbrlReportServiceProvider,
            TaxonomyResolutionService,
            CompanyRepository,
            EditorInformationWorkspaceRepository
        );

    }
    
    /// <summary>
    /// Return the assemblies with the services providers.
    /// </summary>
    /// <returns>Assemblies to evaluate.</returns>
    protected IList<Assembly> GetProvidersAssemblies()
    {
        var assemblies = new List<Assembly>();
        var assembliesStringList = CommonConstants.ProvidersAssembliesList;
        if (!string.IsNullOrEmpty(assembliesStringList))
        {
            var assembliesRawList = JsonConvert.DeserializeObject<List<string>>(assembliesStringList);
            if (assembliesRawList != null)
            {
                foreach (var assembly in assembliesRawList)
                {
                    assemblies.Add(Assembly.Load(assembly));
                }
            }
        }
        return assemblies;
    }
    
    /// <summary>
    /// Add mongo serializers to allow serialization and deserialization of local objects.
    /// </summary>
    private void InitializeMongoSerializers()
    {
        var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || IsAllowedType(type));
        BsonSerializer.RegisterSerializer(objectSerializer);
    }
    /// <summary>
    /// Check if the type is allowed to be serialized.
    /// </summary>
    /// <param name="type">Type to evaluate.</param>
    /// <returns>If is allowed.</returns>
    private static bool IsAllowedType(Type type)
    {
        return type.IsConstructedGenericType ?
            type.GetGenericArguments().All(IsAllowedType) :
            ObjectSerializer.DefaultAllowedTypes(type) || type.FullName.StartsWith("AbaxXBRL");
    }

    protected async Task<RealTimeInstanceDocument> LoadJbrlDocumentByFilingId(string filingGuid = "36756")
    {
        var jbrl = await FilingProcessService.GetRealTimeInstanceDocument(filingGuid);
        var factsBlobsRef = await FilingProcessService.GetReportsFilingBlob(filingGuid, BlobTypeEnum.JbrlFactsByRole);
        if (factsBlobsRef.Success)
        {
            var factsList = new List<RealTimeFact>();
            foreach (var factBlob in factsBlobsRef.Result)
            {
                var iterationFacts = await FilingProcessService.DownloadRealTimeFactsFromBlob(factBlob);
                if (iterationFacts.Success)
                {
                    factsList.AddRange(iterationFacts.Result);
                }
            }
            
            jbrl.Facts = factsList;

            jbrl.Taxonomy =  await TaxonomyResolutionService.GetTaxonomyByNameSpace(jbrl.TaxonomyId);
            
        }

        return jbrl;
    }
    /// <summary>
    ///  | Get the entry point href for the taxonomy
    /// </summary>
    protected async Task<string> GetEntryPointHref(string taxonomyId)
    {
        var taxonomyNameSpaceByEntryPoint = await TaxonomyResolutionService.GetTaxonomyNamespaceByEntryPointHref();
        //based on the taxonomy namespace which is jbrl.TaxonomyId, we get the taxonomy entry point href from the
        //taxonomyNameSpaceByEntryPoint dictionary
        //look for the entry point href (which is the key of the dictionary) in the taxonomyNameSpaceByEntryPoint dictionary using the jbrl.TaxonomyId
        //which is the value of the dictionary
        return taxonomyNameSpaceByEntryPoint.FirstOrDefault(x => x.Value == taxonomyId).Key;
    }
    
}