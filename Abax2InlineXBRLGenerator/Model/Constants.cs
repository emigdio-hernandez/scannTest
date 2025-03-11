namespace Abax2InlineXBRLGenerator.Model;

public class Constants
{
    /// <summary>
    /// The pdf output format.
    /// </summary>
    public const string PDFOutputFormat = "pdf";

    /// <summary>
    /// The ixbrl output format.
    /// </summary>
    public const string IXBRLOutputFormat = "ixbrl";

    /// <summary>
    /// The XBRL period type for duration.
    /// </summary>
    public const string XBRLDurationPeriodType = "duration";

    /// <summary>
    /// The XBRL period type for instant.
    /// </summary>
    public const string XBRLInstantPeriodType = "instant";

    /// <summary>
    /// The XBRL boolean data type.
    /// </summary>
    public const string XBRLBooleanDataType = "boolean";

    /// <summary>
    /// The XBRL string data type.
    /// </summary>
    public const string XBRLStringDataType = "string";

    /// <summary>
    /// The XBRL percent data type.
    /// </summary>
    public const string XBRLPercentDataType = "percent";

    /// <summary>
    /// The XBRL mass data type.
    /// </summary>
    public const string XBRLMassDataType = "mass";

    /// <summary>
    /// The XBRL volume data type.
    /// </summary>
    public const string XBRLVolumeDataType = "volume";

    /// <summary>
    /// The XBRL gregorian calendar year data type.
    /// </summary>
    public const string XBRLGYearDataType = "gYear";

    /// <summary>
    /// The XBRL monetary data type.
    /// </summary>
    public const string XBRLMonetaryDataType = "monetary";

    /// <summary>
    /// The XBRL text block data type.
    /// </summary>
    public const string XBRLTextBlockDataType = "textBlock";

    /// <summary>
    /// The XBRL domain data type.
    /// </summary>
    public const string XBRLDomainDataType = "domain";

    /// <summary>
    /// The XBRL substitution group for item.
    /// </summary>
    public const string XBRLSubstitutionGroupItem = "item";

    /// <summary>
    /// The XBRL substitution group for dimension item.
    /// </summary>
    public const string XBRLSubstitutionGroupDimensionItem = "dimensionItem";

    /// <summary>
    /// The expected date format for XBRL periods.
    /// </summary>
    public const string XBRLPeriodDateFormat = "yyyy-MM-dd";

    /// <summary>
    /// The name of the app setting for the Azure Storage container name for the instance documents.
    /// </summary>
    public const string InstanceDocumentContainerNameAppSetting = "InstanceDocumentContainerName";

    /// <summary>
    /// The name of the app setting for the Azure Storage container name for the output reports.
    /// </summary>
    public const string OutputReportContainerNameAppSetting = "OutputReportContainerName";

    /// <summary>
    /// The XBRL concept for the contextual information.
    /// </summary>
    public const string XBRLOutputFormatiXBRL = "ixbrl";

    /// <summary>
    /// The type of the credit balance.
    /// </summary>
    public const int BalanceCreditType = 2;

    /// <summary>
    /// The type of the debit balance.
    /// </summary>
    public const int BalanceDebitType = 1;

    /// <summary>
    /// The credit balance.
    /// </summary>
    public const string BalanceCredit = "credit";

    /// <summary>
    /// The debit balance.
    /// </summary>
    public const string BalanceDebit = "debit";

    /// <summary>
    /// The XBRL monetary item type.
    /// </summary>
    public const string XBRLMonetaryItemType = "http://www.xbrl.org/2003/instance:monetaryItemType";

    /// <summary>
    /// The XBRL token item type.
    /// </summary>
    public const string XBRLTokenItemType = "http://www.xbrl.org/2003/instance:tokenItemType";

    /// <summary>
    /// The XBRL text block item type.
    /// </summary>
    public const string XBRLTextBlockItemType = "http://www.xbrl.org/dtr/type/non-numeric:textBlockItemType";

    /// <summary>
    /// The XBRL string item type.
    /// </summary>
    public const string XBRLStringItemType = "http://www.xbrl.org/2003/instance:stringItemType";

    /// <summary>
    /// The XBRL date item type.
    /// </summary>
    public const string XBRLDateItemType = "http://www.xbrl.org/2003/instance:dateItemType";

    /// <summary>
    /// The XBRL per share item type.
    /// </summary>
    public const string XBRLPerShareItemType = "http://www.xbrl.org/dtr/type/numeric:perShareItemType";

    /// <summary>
    /// The XBRL decimal item type.
    /// </summary>
    public const string XBRLDecimalItemType = "http://www.xbrl.org/2003/instance:decimalItemType";

    /// <summary>
    /// The XBRL boolean item type.
    /// </summary>
    public const string XBRLBooleanItemType = "http://www.xbrl.org/2003/instance:booleanItemType";

    /// <summary>
    /// The XBRL integer item type.
    /// </summary>
    public const string XBRLIntegerItemType = "http://www.xbrl.org/2003/instance:integerItemType";
    
    /// <summary>
    /// The XBRL non negative integer item type.
    /// </summary>
    public const string XBRLNonNegativeIntegerItemType = "http://www.xbrl.org/2003/instance:nonNegativeIntegerItemType";

    /// <summary>
    /// The XBRL float item type.
    /// </summary>
    public const string XBRLFloatItemType = "http://www.xbrl.org/2003/instance:floatItemType";

    /// <summary>
    /// The XBRL double item type.
    /// </summary>
    public const string XBRLDoubleItemType = "http://www.xbrl.org/2003/instance:doubleItemType";

    /// <summary>
    /// The XBRL fraction item type.
    /// </summary>
    public const string XBRLFractionItemType = "http://www.xbrl.org/2003/instance:fractionItemType";

    /// <summary>
    /// The XBRL normalized string item type.
    /// </summary>
    public const string XBRLNormalizedStringItemType = "http://www.xbrl.org/2003/instance:normalizedStringItemType";

    /// <summary>
    /// The XBRL date time item type.
    /// </summary>
    public const string XBRLDateTimeItemType = "http://www.xbrl.org/2003/instance:dateTimeItemType";

    /// <summary>
    /// The XBRL time item type.
    /// </summary>
    public const string XBRLTimeItemType = "http://www.xbrl.org/2003/instance:timeItemType";

    /// <summary>
    /// The XBRL duration item type.
    /// </summary>
    public const string XBRLDurationItemType = "http://www.xbrl.org/2003/instance:durationItemType";

    /// <summary>
    /// The XBRL g year item type.
    /// </summary>
    public const string XBRLGYearItemType = "http://www.xbrl.org/2003/instance:gYearItemType";

    /// <summary>
    /// The XBRL g month day item type.
    /// </summary>
    public const string XBRLGMonthDayItemType = "http://www.xbrl.org/2003/instance:gMonthDayItemType";

    /// <summary>
    /// The XBRL g month item type.
    /// </summary>
    public const string XBRLGMonthItemType = "http://www.xbrl.org/2003/instance:gMonthItemType";

    /// <summary>
    /// The XBRL g day item type.
    /// </summary>
    public const string XBRLGDayItemType = "http://www.xbrl.org/2003/instance:gDayItemType";

    /// <summary>
    /// The XBRL g year month item type.
    /// </summary>
    public const string XBRLGYearMonthItemType = "http://www.xbrl.org/2003/instance:gYearMonthItemType";

    /// <summary>
    /// The XBRL any URI item type.
    /// </summary>
    public const string XBRLAnyURIItemType = "http://www.xbrl.org/2003/instance:anyURIItemType";

    /// <summary>
    /// The XBRL QName item type.
    /// </summary>
    public const string XBRLQNameItemType = "http://www.xbrl.org/2003/instance:QNameItemType";

    /// <summary>
    /// The XBRL XML item type.
    /// </summary>
    public const string XBRLXMLItemType = "http://www.xbrl.org/2003/instance:XMLItemType";

    /// <summary>
    /// The XBRL shares item type.
    /// </summary>
    public const string XBRLSharesItemType = "http://www.xbrl.org/dtr/type/numeric:sharesItemType";
    /// <summary>
    /// The XBRL shares item type.
    /// </summary>
    public const string XBRLSharesItemType2003 = "http://www.xbrl.org/2003/instance:sharesItemType";
    

    /// <summary>
    /// The XBRL pure item type.
    /// </summary>
    public const string XBRLPureItemType = "http://www.xbrl.org/2003/instance:pureItemType";

    /// <summary>
    /// The XBRL percent item type.
    /// </summary>
    public const string XBRLPercentItemType = "http://www.xbrl.org/2003/instance:percentItemType";
    /// <summary>
    /// The quarterly financial report type.
    /// </summary>
    public const string QuarterlyFinancialReportType = "quarterlyFinancialReportType";
    /// <summary>
    /// The annual report type.
    /// </summary>
    public const string AnnualReportType = "annualReportType";
    /// <summary>
    /// The semi annual report type.
    /// </summary>
    public const string SemiAnnualReportType = "semiAnnualReportType";
    /// <summary>
    /// The monthly report type.
    /// </summary>
    public const string MonthlyReportType = "monthlyReportType";
    /// <summary>
    /// The eventual report type.
    /// </summary>
    public const string EventualReportType = "eventualReportType";

}
