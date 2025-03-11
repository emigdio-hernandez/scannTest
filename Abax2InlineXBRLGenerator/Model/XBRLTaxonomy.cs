using Newtonsoft.Json;
using AbaxXBRLCore.Dto.InstanceEditor.Dto;
using Newtonsoft.Json.Serialization;

namespace Abax2InlineXBRLGenerator.Model;

/// <summary>
/// Represents an XBRL taxonomy.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class XBRLTaxonomy
{
    /// <summary>
    /// The ID of the taxonomy.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The URI of the entry point of the taxonomy.
    /// </summary>
    [JsonProperty("epUri")]
    public string EntryPointUri { get; set; }
    /// <summary>
    /// Main namespace of this taxonomy entry point
    /// </summary>
    [JsonProperty("ns")]
    public string TaxonomyNameSpace { get; set; }
    /// <summary>
    /// The name of the taxonomy.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// The version of the taxonomy.
    /// </summary>
    [JsonProperty("ver")]
    public string Version { get; set; }

    /// <summary>
    /// The concepts of the taxonomy.
    /// </summary>
    public IDictionary<string, XBRLConcept> Concepts { get; set; }

    /// <summary>
    /// The labels of the concepts in the taxonomy ordered by language and role and concept ID.
    /// </summary>

    public IDictionary<string, IDictionary<string, IDictionary<string, XBRLLabel>>> Labels { get; set; }

    /// <summary>
    /// The labels of the roles in the taxonomy ordered by roleUri and language.
    /// </summary>
    public IDictionary<string, IDictionary<string, XBRLLabel>> RoleLabels { get; set; }

    /// <summary>
    /// The presentation linkbases of the taxonomy ordered by role.
    /// </summary>
    [JsonProperty("preLbs")]
    public IDictionary<string, XBRLPresentationLinkbase> PresentationLinkbases { get; set; }


    /// <summary>
    /// The Calculation linkbases of the taxonomy ordered by role.
    /// </summary>
    [JsonProperty("calcLbs")]
    public IDictionary<string, XBRLCalculationLinkbase> CalculationLinkbases { get; set; }
    /// <summary>
    /// The prefixes used in the taxonomy with their corresponding namespaces.
    /// </summary>
    [JsonProperty("pfxs")]
    public IDictionary<string, string> Prefixes { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLTaxonomy"/> class.
    /// </summary>
    /// <param name="id">the id of the taxonomy</param>
    /// <param name="taxonomyNamespace">the namespace of the entry poing of the taxonomy, this namespace acts as
    /// the main namespace of the taxonomy</param>
    /// <param name="entryPointUri">the URI of the entry point of the taxonomy</param>
    /// <param name="name">the name of the taxonomy</param>
    /// <param name="version">the version of the taxonomy</param>
    public XBRLTaxonomy(string id, string taxonomyNamespace, string entryPointUri, string name, string version)
    {
        Id = id;
        TaxonomyNameSpace = taxonomyNamespace;
        EntryPointUri = entryPointUri;
        Name = name;
        Version = version;
        Concepts = new Dictionary<string, XBRLConcept>();
        Labels = new Dictionary<string, IDictionary<string, IDictionary<string, XBRLLabel>>>();
        PresentationLinkbases = new Dictionary<string, XBRLPresentationLinkbase>();
        RoleLabels = new Dictionary<string, IDictionary<string, XBRLLabel>>();
        Prefixes = new Dictionary<string, string>();
        CalculationLinkbases = new Dictionary<string, XBRLCalculationLinkbase>();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="XBRLTaxonomy"/> class.
    /// </summary>
    /// <param name="id">the id of the taxonomy</param>
    /// <param name="taxonomyNamespace">the namespace of the entry poing of the taxonomy, this namespace acts as
    /// the main namespace of the taxonomy</param>
    /// <param name="entryPointUri">the URI of the entry point of the taxonomy</param>
    /// <param name="name">the name of the taxonomy</param>
    /// <param name="version">the version of the taxonomy</param>
    /// <param name="taxonomy">The DTO with taxonomy of reference </param>
    public XBRLTaxonomy(string id, string taxonomyNamespace, string entryPointUri, string name, string version, TaxonomiaDto taxonomy)
    {
        Id = id;
        TaxonomyNameSpace = taxonomyNamespace;
        EntryPointUri = entryPointUri;
        Name = name;
        Version = version;
        Concepts = new Dictionary<string, XBRLConcept>();
        Labels = new Dictionary<string, IDictionary<string, IDictionary<string, XBRLLabel>>>();
        PresentationLinkbases = new Dictionary<string, XBRLPresentationLinkbase>();
        CalculationLinkbases = new Dictionary<string, XBRLCalculationLinkbase>();
        RoleLabels = new Dictionary<string, IDictionary<string, XBRLLabel>>();
        foreach (var (_, concept) in taxonomy.ConceptosPorId)
        {
            Concepts.Add(concept.Id, new XBRLConcept(concept.Id, concept.Nombre, concept.EspacioNombres, concept.TipoPeriodo, concept.Balance == Constants.BalanceCreditType ? Constants.BalanceCredit : Constants.BalanceDebit, 
            concept.TipoDato, concept.TipoDatoXbrl, concept.Tipo, concept.EsAbstracto, concept.EsDimension && concept.AtributosAdicionales != null && concept.AtributosAdicionales.Count > 0, 
            concept.EsNillable, concept.EsDimension, concept.EsMiembroDimension, null, concept.AtributosAdicionales));

            if(concept.Referencias != null)
            {
                foreach (var reference in concept.Referencias)
                {
                    var xbrlRef = new XBRLReference();
                    xbrlRef.Name = reference.Rol;

                    foreach (var referencePart in reference.Partes)
                    {
                        if(!taxonomy.PrefijosTaxonomia.TryGetValue(referencePart.EspacioNombres, out var partPrefix))
                        {
                            partPrefix = referencePart.EspacioNombres;
                        }
                     
                        xbrlRef.ReferenceParts.Add(new XBRLReferencePart(referencePart.Nombre,partPrefix ,referencePart.Valor));
                    }
                    Concepts[concept.Id].References.Add(xbrlRef);
                }
            }

            if (concept.Etiquetas != null)
            {
                foreach (var (language, labels) in concept.Etiquetas)
                {
                    if (!Labels.ContainsKey(language))
                    {
                        Labels.Add(language, new Dictionary<string, IDictionary<string, XBRLLabel>>());
                    }
                    foreach (var (role, roleLabel) in labels)
                    {
                        if (!Labels[language].ContainsKey(role))
                        {
                            Labels[language].Add(role, new Dictionary<string, XBRLLabel>());
                        }
                        if (!Labels[language][role].ContainsKey(concept.Id))
                        {
                            Labels[language][role].Add(concept.Id, new XBRLLabel(roleLabel.Rol, roleLabel.Idioma, roleLabel.Valor));
                        }
                    }
                }
            }
        }

        foreach (var (_, hypercubes) in taxonomy.ListaHipercubos)
        {
            if (hypercubes == null)
            {
                continue;
            }
            foreach (var hypercupe in hypercubes)
            {
                foreach (var (dimensionId, dimensionStructures) in hypercupe.EstructuraDimension)
                {
                    VisitDimensionStructures(dimensionId, dimensionStructures);
                }
            }
        }

        foreach (var presentationLinkbaseStructure in taxonomy.RolesPresentacion)
        {
            if (presentationLinkbaseStructure == null)
            {
                continue;
            }
            if (!PresentationLinkbases.ContainsKey(presentationLinkbaseStructure.Uri))
            {
                PresentationLinkbases.Add(presentationLinkbaseStructure.Uri, new XBRLPresentationLinkbase(presentationLinkbaseStructure.Nombre, presentationLinkbaseStructure.Uri));
            }
            int indent = 0;
            ProcessPresentationLinkbaseStructures(presentationLinkbaseStructure.Uri, presentationLinkbaseStructure.Estructuras, indent);
        }

        //fill calculation linkbases
        
        foreach (var calculationLinkbase in taxonomy.RolesCalculo)
        {
            if (calculationLinkbase == null)
            {
                continue;
            }

            var newCalculationLinkbase = new XBRLCalculationLinkbase();
            CalculationLinkbases[calculationLinkbase.Uri] = newCalculationLinkbase;
            foreach (var (conceptId, calculationItems) in calculationLinkbase.OperacionesCalculo)
            {
                newCalculationLinkbase.Calculations[conceptId] = new List<XBRLCalculationSummationItem>();
                foreach (var calculationItem in calculationItems)
                {
                    newCalculationLinkbase.Calculations[conceptId].Add(
                        new XBRLCalculationSummationItem(calculationItem.IdConcepto, calculationItem.Peso)
                        );
                }
            }
        }
        
        // labels in the taxonomy are ordered by language and then roleId, check if the role has labels for the specified language
        foreach (var (language, roleLabel) in taxonomy.EtiquetasRol)
        {
            foreach (var (roleUri, label) in roleLabel)
            {
                if (!RoleLabels.ContainsKey(roleUri))
                {
                    RoleLabels.Add(roleUri, new Dictionary<string, XBRLLabel>());
                }
                if (!RoleLabels[roleUri].ContainsKey(language))
                {
                    RoleLabels[roleUri].Add(language, new XBRLLabel(string.Empty, language, label.Valor));
                }
            }
        }
        

        Prefixes = new Dictionary<string, string>();
        foreach (var (namespaceUri, prefix) in taxonomy.PrefijosTaxonomia)
        {
            if (Prefixes.ContainsKey(prefix))
            {
                Prefixes[prefix] = namespaceUri;
            }
            else
            {
                Prefixes.Add(prefix, namespaceUri);
            }

        }
    }

    private void ProcessPresentationLinkbaseStructures(string roleUri, IList<EstructuraFormatoDto>? presentationLinkbaseStructures, int indent)
    {
        if (presentationLinkbaseStructures == null)
        {
            return;
        }
        foreach (var item in presentationLinkbaseStructures)
        {
            var presentationLinkbaseItem = new XBRLPresentationLinkbaseItem
            {
                ConceptId = item.IdConcepto,
                LabelRole = item.RolEtiquetaPreferido,
                Indentation = indent,
                IsAbstract = Concepts.ContainsKey(item.IdConcepto) && Concepts[item.IdConcepto].IsAbstract,
                ChildrenCount = item.SubEstructuras?.Count ?? 0
            };
            PresentationLinkbases[roleUri].PresentationLinkbaseItems.Add(presentationLinkbaseItem);
            if (item.SubEstructuras != null)
            {
                ProcessPresentationLinkbaseStructures(roleUri, item.SubEstructuras, indent + 1);
            }
        }
    }

    private void VisitDimensionStructures(string dimensionId, IList<EstructuraFormatoDto>? dimensionStructures)
    {
        if (dimensionStructures == null)
        {
            return;
        }
        foreach (var dimensionStructure in dimensionStructures)
        {
            if (dimensionStructure == null)
            {
                continue;
            }
            if (Concepts.ContainsKey(dimensionStructure.IdConcepto))
            {
                Concepts[dimensionStructure.IdConcepto].ParentDimension = dimensionId;
            }
            VisitDimensionStructures(dimensionId, dimensionStructure.SubEstructuras);
        }
    }

    /// <summary>
    /// Gets the label for a concept in the taxonomy for a given language and role. If the label is not found, a default label is returned.
    /// </summary>
    /// <param name="conceptId">the ID of the concept</param>
    /// <param name="language">the language of the label</param>
    /// <param name="role">the role of the label</param>
    /// <param name="defaultLabel">an optional default label to return if the label is not found</param>
    /// <returns>the label for the concept, or a default label if not found</returns>
    public XBRLLabel GetConceptLabel(string conceptId, string language, string role, string? defaultLabel = null)
    {
        XBRLLabel? label = null;
        // labels are ordered by language, role and then conceptId, check if the concept has labels for the specified language and role
        if (Labels.ContainsKey(language))
        {
            if (Labels[language].ContainsKey(role))
            {
                if (Labels[language][role].ContainsKey(conceptId))
                {
                    label = Labels[language][role][conceptId];
                }
            }
        }
        return label ?? new XBRLLabel(role, language, defaultLabel ?? string.Empty);
    }

}