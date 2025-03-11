using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace Abax2InlineXBRLGenerator.Util;

/// <summary>
/// Extracts visible text from HTML content.
/// </summary>
public class HtmlTextExtractor
{
    /// <summary>
    /// Extracts visible text from HTML content.
    /// </summary>
    /// <param name="html">The HTML content.</param>
    /// <returns>The visible text.</returns>
    public static string ExtractVisibleText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var sb = new StringBuilder();

        ExtractTextFromNode(doc.DocumentNode, sb, 0);

        string result = sb.ToString().Trim();

        // Reemplazar &nbsp; por espacio
        result = result.Replace("&nbsp;", " ");

        // Reemplazar múltiples espacios en blanco por uno solo
        //result = Regex.Replace(result, @"\s{2,}", " ");

        // Reemplazar múltiples saltos de línea por uno solo
        result = Regex.Replace(result, @"(\r\n|\n){3,}", Environment.NewLine);

        return HttpUtility.HtmlDecode(result);
    }

    /// <summary>
    /// Extracts visible text from an HTML node.
    /// </summary>
    /// <param name="node">The HTML node.</param>
    /// <param name="sb">The string builder.</param>
    /// <param name="level">The level.</param>
    private static void ExtractTextFromNode(HtmlNode node, StringBuilder sb, int level)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = node.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (node.ParentNode.Name == "span" || node.ParentNode.Name == "a" || node.ParentNode.Name == "i")
                {
                    sb.Append(text + " ");
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine(text);
                }
            }
        }
        else if (node.Name == "h1" || node.Name == "h2" || node.Name == "h3" || node.Name == "h4" || node.Name == "h5" || node.Name == "h6")
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(node.InnerText.Trim().ToUpper());
        }
        else if (node.Name == "li")
        {
            sb.AppendLine();
            sb.AppendLine();
            var prefix = node.ParentNode.Name == "ol" ? $"{level + 1}. " : "- ";
            sb.Append($"{prefix}{node.InnerText.Trim()}");
        }
        else if (node.Name == "table")
        {
            ExtractTableContent(node, sb);
        }
        else
        {
            int childLevel = node.Name == "ol" || node.Name == "ul" ? 0 : level;
            foreach (var childNode in node.ChildNodes)
            {
                ExtractTextFromNode(childNode, sb, childLevel);
                if (node.Name == "ol" || node.Name == "ul")
                {
                    childLevel++;
                }
            }
        }

        if (node.Name == "p" || node.Name == "div" || node.Name == "ul" || node.Name == "ol" || node.Name == "table")
        {
            sb.AppendLine();
        }
    }

    /// <summary>
    /// Extracts the content of a table.
    /// </summary>
    /// <param name="tableNode">The table node.</param>
    /// <param name="sb">The string builder.</param>
    /// <returns>The table content.</returns>
    private static void ExtractTableContent(HtmlNode tableNode, StringBuilder sb)
    {
        var rows = tableNode.SelectNodes(".//tr");
        if (rows == null) return;

        sb.AppendLine();
        foreach (var row in rows)
        {
            var cells = row.SelectNodes(".//th|.//td");
            if (cells == null) continue;

            sb.AppendLine(string.Join("\t| ", cells.Select(cell => cell.InnerText.Trim())));
        }
        sb.AppendLine();
    }
}
