using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
{
    private void PostProcess(List<NodeWithDetails> alreadyProcessed)
    {
        // If some identifiers weren't recognized at first attempt, but later instead
        // then we may fix the previous ones.
        var identifiers = alreadyProcessed.Where(x => x.Colour == NodeColors.DefaultColour).ToList();

        foreach (var entry in identifiers)
        {
            if (entry.SkipIdentifierPostProcessing)
                continue;

            if (IdentifierShouldntBeOverriden(entry, alreadyProcessed))
                continue;

            if (_FoundInterfaces.Contains(entry.Text))
                entry.Colour = NodeColors.Interface;

            if (_FoundClasses.Contains(entry.Text))
                entry.Colour = NodeColors.Class;

            if (_FoundStructs.Contains(entry.Text))
                entry.Colour = NodeColors.Struct;

            if (_FoundPropertiesOrFields.Contains(entry.Text))
                entry.Colour = NodeColors.PropertyName;

            entry.ClassificationType = MapColourToClassificationType(entry.Colour, entry.ClassificationType);
        }

        var identifiersOrDefaults = alreadyProcessed
            .Where(x => x.Colour == NodeColors.Identifier || x.Colour == NodeColors.DefaultColour);

        var initialCount = identifiersOrDefaults.Count();
        var newCount = 0;
        var antiInfiniteLoopCheck = 0;

        while(antiInfiniteLoopCheck < 100)
        {
            antiInfiniteLoopCheck++;

            foreach (var entry in identifiersOrDefaults)
            {
                if (MarkIdentifierAsNamespaceIfThereIsClassAhead(entry))
                {
                    entry.Colour = NodeColors.Namespace;
                    entry.ClassificationType = ClassificationTypeNames.NamespaceName;
                }
            }

            newCount = identifiersOrDefaults.Count();

            if (newCount != initialCount)
            {
                initialCount = newCount;
                continue;
            }
            else
            {
                break;
            }
        }

        // Fix namespaces
        var namespaces = alreadyProcessed
        .Where(x => x.Colour == NodeColors.Namespace && IsClassOrStructAlreadyFound(x.Text))
        .ToList();

        if (namespaces.Any())
            FixNamespacesThatAreClasses(namespaces);
    }

    private bool MarkIdentifierAsNamespaceIfThereIsClassAhead(NodeWithDetails entry)
    {
        if (entry.ClassificationType != ClassificationTypeNames.Identifier && entry.Colour != NodeColors.DefaultColour)
            return false;

        var i = _Output.FindIndex(x => x.Id == entry.Id);

        if (i < 0 || (i + 2) >= _Output.Count)
            return false;

        if (_Output[i + 1].Text == "." && _Output[i + 2].ClassificationType.EqualsAnyOf
        (
            ClassificationTypeNames.NamespaceName,
            ClassificationTypeNames.ClassName,
            ClassificationTypeNames.StructName,
            ClassificationTypeNames.RecordClassName,
            ClassificationTypeNames.RecordStructName
        ))
        {
            return true;
        }

        return false;
    }

    private void FixNamespacesThatAreClasses(List<NodeWithDetails> namespaces)
    {
        foreach (var ns in namespaces)
        {
            if (ShouldNamespaceBeAdjusted(ns))
            {
                ns.Colour = ResolveName(ns.Text);
                MarkNextChainElementsToProperty(ns);
            }
        }
    }

    private bool ShouldNamespaceBeAdjusted(NodeWithDetails ns)
    {
        var i = _Output.FindIndex(x => x.Id == ns.Id) - 1;

        if (i < 0)
            return false;

        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.NamespaceName,
        };

        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier

        var state = 0;

        for (; i >= 0; i--)
        {
            var current = _Output[i];

            if (state == 0)
            {
                if (current.Text == ".")
                {
                    state = 1;
                    continue;
                }
                else
                {
                    if (current.Text.EqualsAnyOf("namespace", "using"))
                        return false;

                    if (AccessibilityModifiers.Contains(current.Text))
                        return false;

                    return true;
                }
            }
            else
            {
                if (valid_identifiers.Contains(current.ClassificationType))
                {
                    state = 0;
                    continue;
                }
                else
                {
                    if (current.Text.EqualsAnyOf("namespace", "using"))
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        return false;
    }

    private void MarkNextChainElementsToProperty(NodeWithDetails ns)
    {
        var i = _Output.FindIndex(x => x.Id == ns.Id);

        if (i < 0)
            return;

        i++;

        if (i >= _Output.Count)
            return;

        var valid_identifiers = new List<string>
        {
            ClassificationTypeNames.Identifier,
            ClassificationTypeNames.NamespaceName,
        };

        // 0 = currently at Identifier, expecting Operator
        // 1 = currently at Operator, expecting Identifier

        var state = 0;

        for (; i < _Output.Count; i++)
        {
            var current = _Output[i];
            if (state == 0)
            {
                if (current.Text == ".")
                {
                    state = 1;
                    continue;
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (current.ClassificationType == ClassificationTypeNames.MethodName)
                    return;

                if (current.ClassificationType == ClassificationTypeNames.Punctuation)
                    return;

                if (current.ClassificationType == ClassificationTypeNames.Identifier)
                {
                    current.Colour = NodeColors.PropertyName;
                    current.ClassificationType = ClassificationTypeNames.PropertyName;
                    state = 0;
                    continue;
                }

                if (current.ClassificationType.EqualsAnyOf(
                    ClassificationTypeNames.ClassName,
                    ClassificationTypeNames.StructName))
                {
                    current.Colour = NodeColors.PropertyName;
                    current.ClassificationType = ClassificationTypeNames.PropertyName;
                    state = 0;
                    return;
                }

                if (valid_identifiers.Contains(current.ClassificationType))
                {
                    state = 0;
                    continue;
                }
                else
                {
                    return;
                }
            }
        }
    }

    private bool IdentifierShouldntBeOverriden(NodeWithDetails entry, List<NodeWithDetails> nodes)
    {
        var index = nodes.IndexOf(entry);

        if (index == -1)
            return false;

        if (nodes.IndexIsValid(index + 1, out var node) &&
            node.ClassificationType == ClassificationTypeNames.Operator &&
            node.Text.Contains("="))
            return true;

        if (nodes.IndexIsValid(index + 1, out node) && node.Text == ".")
            return true;

        return false;
    }
}
