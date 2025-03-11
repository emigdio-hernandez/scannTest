using System;
using Abax2InlineXBRLGenerator.Model;
using Newtonsoft.Json;

namespace Abax2InlineXBRLGenerator.Generator;

/// <summary>
/// Extension methods for TemplateContext to handle else block processing.
/// </summary>
public static class TemplateContextExtensions
{
    private const string PENDING_ELSE_BLOCKS_KEY = "PendingElseBlocks";
    private const string SKIPPED_ELSE_BLOCKS_KEY = "SkippedElseBlocks";

    /// <summary>
    /// Registers an else block as pending for processing.
    /// </summary>
    /// <param name="context">The template processing context</param>
    /// <param name="elseId">The unique ID of the else block</param>
    public static void RegisterPendingElseBlock(this TemplateContext context, string elseId)
    {
        var pendingBlocks = GetPendingElseBlocks(context);
        var varName = PENDING_ELSE_BLOCKS_KEY;
        var pendingBlocksVar = context.GetVariable(varName);

        if (pendingBlocksVar != null)
        {
            pendingBlocks.Add(elseId);
            pendingBlocksVar.SetValue(JsonConvert.SerializeObject(pendingBlocks));
            context.SetVariableValue(varName, pendingBlocksVar);
        }
    }

    /// <summary>
    /// Marks an else block as skipped, preventing it from being processed.
    /// </summary>
    /// <param name="context">The template processing context</param>
    /// <param name="elseId">The unique ID of the else block</param>
    public static void MarkElseBlockSkipped(this TemplateContext context, string elseId)
    {
        var skippedBlocks = GetSkippedElseBlocks(context);

        var varName = SKIPPED_ELSE_BLOCKS_KEY;
        var skippedBlocksVar = context.GetVariable(varName);

        if (skippedBlocksVar != null)
        {
            skippedBlocks.Add(elseId);
            skippedBlocksVar.SetValue(JsonConvert.SerializeObject(skippedBlocks));
            context.SetVariableValue(varName, skippedBlocksVar);
        }
    }

    /// <summary>
    /// Checks if an else block should be processed based on its current status.
    /// </summary>
    /// <param name="context">The template processing context</param>
    /// <param name="elseId">The unique ID of the else block</param>
    /// <returns>True if the else block should be processed, false otherwise</returns>
    public static bool ShouldProcessElseBlock(this TemplateContext context, string elseId)
    {
        var pendingBlocks = GetPendingElseBlocks(context);
        var skippedBlocks = GetSkippedElseBlocks(context);

        return pendingBlocks.Contains(elseId) && !skippedBlocks.Contains(elseId);
    }

    /// <summary>
    /// Clears the status of an else block, allowing it to be processed again.
    /// </summary>
    /// <param name="context">The template processing context</param>
    /// <param name="elseId">The unique ID of the else block</param>
    public static void ClearElseBlockStatus(this TemplateContext context, string elseId)
    {
        var pendingBlocks = GetPendingElseBlocks(context);
        var skippedBlocks = GetSkippedElseBlocks(context);

        var varName = PENDING_ELSE_BLOCKS_KEY;
        var pendingBlocksVar = context.GetVariable(varName);

        if (pendingBlocksVar != null)
        {
            pendingBlocks.Remove(elseId);
            pendingBlocksVar.SetValue(JsonConvert.SerializeObject(pendingBlocks));
            context.SetVariableValue(varName, pendingBlocksVar);
        }

        varName = SKIPPED_ELSE_BLOCKS_KEY;
        var skippedBlocksVar = context.GetVariable(varName);

        if (skippedBlocksVar != null)
        {
            skippedBlocks.Remove(elseId);
            skippedBlocksVar.SetValue(JsonConvert.SerializeObject(skippedBlocks));
            context.SetVariableValue(varName, skippedBlocksVar);
        }
    }

    /// <summary>
    /// Gets the set of pending else blocks from the context.
    /// </summary>
    /// <param name="context">The template processing context</param>
    /// <returns>The set of pending else blocks</returns>
    private static HashSet<string> GetPendingElseBlocks(TemplateContext context)
    {
        var varName = PENDING_ELSE_BLOCKS_KEY;
        var pendingBlocksVar = context.GetVariable(varName);
        
        if (pendingBlocksVar == null)
        {
            var newVar = new TemplateVariable(varName, TemplateVariable.VariableType.String, TemplateVariable.VariableScope.Local);
            newVar.SetValue(JsonConvert.SerializeObject(new HashSet<string>()));
            context.SetVariableValue(varName, newVar);
            pendingBlocksVar = newVar;
        }

        return JsonConvert.DeserializeObject<HashSet<string>>(pendingBlocksVar.GetValue<string>() ?? "[]") ?? new HashSet<string>();
    }

    /// <summary>
    /// Gets the set of skipped else blocks from the context.
    /// </summary>
    /// <param name="context">The template processing context</param>
    /// <returns>The set of skipped else blocks</returns>
    private static HashSet<string> GetSkippedElseBlocks(TemplateContext context)
    {
        var varName = SKIPPED_ELSE_BLOCKS_KEY;
        var skippedBlocksVar = context.GetVariable(varName);
        
        if (skippedBlocksVar == null)
        {
            var newVar = new TemplateVariable(varName, TemplateVariable.VariableType.String, TemplateVariable.VariableScope.Local);
            newVar.SetValue(JsonConvert.SerializeObject(new HashSet<string>()));
            context.SetVariableValue(varName, newVar);
            skippedBlocksVar = newVar;
        }

        return JsonConvert.DeserializeObject<HashSet<string>>(skippedBlocksVar.GetValue<string>() ?? "[]") ?? new HashSet<string>();
    }
}