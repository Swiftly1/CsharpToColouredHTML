using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
{
    private ExtractedColourResult HandlePunctuation(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

        if (node.Text == "{")
        {
            _BracketsCounter++;

            // void Test() {
            if (currentIndex > 2 && nodes[currentIndex - 1].Text == ")")
            {
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    if (i > 0 && nodes[i].Text == "(" && nodes[i - 1].ClassificationType == ClassificationTypeNames.MethodName)
                        _IsWithinMethod = true;
                }
            }
        }

        if (node.Text == "}")
        {
            _BracketsCounter--;

            if (_BracketsCounter <= 0)
                _IsWithinMethod = false;
        }

        if (node.Text == "(")
        {
            _ParenthesisCounter++;

            if (_IsNew && _ParenthesisCounter == 1)
                _IsNew = false;
        }

        if (node.Text == ")")
        {
            _ParenthesisCounter--;

            if (_ParenthesisCounter <= 0)
                _IsUsing = false;

            if (_ParenthesisCounter <= 0)
                _IsNew = false;

            if (_ParenthesisCounter <= 0)
                _IsTypeOf = false;
        }

        if (node.Text == ";")
        {
            _IsUsing = false;
            _IsNew = false;
            _IsTypeOf = false;
        }

        return new ExtractedColourResult(NodeColors.Punctuation);
    }

    private ExtractedColourResult HandleIdentifier(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

        var isInterfaceResult = IsInterface(currentIndex, nodes);
        if (isInterfaceResult.Detected)
        {
            _FoundInterfaces.Add(node.Text);
            return new ExtractedColourResult(NodeColors.Interface, isInterfaceResult.SkipPostProcessing);
        }

        var isMethodResult = IsMethod(currentIndex, nodes);
        if (isMethodResult.Detected)
        {
            TryUpdatePreviousIdentifierToClassIfThatWasNamespace(currentIndex, nodes);
            return new ExtractedColourResult(NodeColors.Method, isMethodResult.SkipPostProcessing);
        }

        var isClassOrStructResult = IsClassOrStruct(currentIndex, nodes);
        if (isClassOrStructResult.Detected)
        {
            if (IsPopularStruct(node.Text))
            {
                _FoundStructs.Add(node.Text);
                return new ExtractedColourResult(NodeColors.Struct, isClassOrStructResult.SkipPostProcessing);
            }
            else if (IdentifierFirstCharCaseSeemsLikeVariable(node.Text))
            {
                if (ThereIsThisInTheChainBefore(currentIndex, nodes))
                {
                    return new ExtractedColourResult(NodeColors.Identifier, true);
                }
                else
                {
                    // If Roslyn does not see it as a local variable because e.g declaration is not in the source code
                    // Then we have to decide whether this is local variable or property or field, etc.
                    // Then I think we should default to Property, but if it is top-level-statements like syntax,
                    // then I'd use LocalName because the result is better to read.
                    // Basically if we do not see declaration and it starts with lower case and is outside method body,
                    // then it should be Property due to popularity.

                    if (node.ClassificationType == ClassificationTypeNames.Identifier && _IsWithinMethod)
                        return new ExtractedColourResult(NodeColors.PropertyName, true);

                    return new ExtractedColourResult(NodeColors.LocalName);
                }
            }
            else
            {
                if (_FoundPropertiesOrFields.Contains(node.Text))
                {
                    return new ExtractedColourResult(NodeColors.PropertyName);
                }
                else
                {
                    AddClass(node.Text);
                    return new ExtractedColourResult(NodeColors.Class, isClassOrStructResult.SkipPostProcessing);
                }
            }
        }


        var isPropertyOrFieldResult = IsPropertyOrField(currentIndex, nodes);
        if (isPropertyOrFieldResult.Detected)
        {
            if (IdentifierFirstCharCaseSeemsLikeVariable(node.Text))
            {
                if (ThereIsThisInTheChainBefore(currentIndex, nodes) ||
                    currentIndex > 0 && nodes[currentIndex - 1].Text == ".")
                {
                    return new ExtractedColourResult(NodeColors.Identifier, true);
                }
                else
                {
                    // If Roslyn does not see it as a local variable because e.g declaration is not in the source code
                    // Then we have to decide whether this is local variable or property or field, etc.
                    // I think we should default to Property, but if it is top-level-statements like syntax,
                    // then I'd use LocalName because the result is better to read.
                    // Basically if we do not see declaration and it starts with lower case and is outside method body,
                    // then it should be Property due to popularity.

                    if (node.ClassificationType == ClassificationTypeNames.Identifier && _IsWithinMethod)
                        return new ExtractedColourResult(NodeColors.PropertyName, true);

                    return new ExtractedColourResult(NodeColors.LocalName, isPropertyOrFieldResult.SkipPostProcessing);
                }
            }
            else
            {
                return new ExtractedColourResult(NodeColors.Identifier, true);
            }
        }
        else
        {
            if (currentIndex + 1 < nodes.Count &&
                nodes[currentIndex + 1].ClassificationType != ClassificationTypeNames.Operator &&
                nodes[currentIndex + 1].Text != "." &&
                !_IsUsing)
            {
                if (CheckIfChainIsMadeOfVariablesColors(currentIndex, _Output))
                {
                    AddClass(node.Text);
                    return new ExtractedColourResult(NodeColors.Class);
                }
            }

            if (_FoundPropertiesOrFields.Contains(node.Text))
                return new ExtractedColourResult(NodeColors.PropertyName);

            return new ExtractedColourResult(NodeColors.Identifier);
        }
    }

    private DetectionStatus IsInterface(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);
        var canGoTwoAhead = nodes.CanGoAhead(currentIndex, 2);

        var startsWithI = node.Text.StartsWith("I") && node.Text.Length > 1 && char.IsUpper(node.Text[1]);

        if (startsWithI && canGoBehind && new[] { ":", "<" }.Contains(nodes[currentIndex - 1].Text))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (startsWithI && canGoBehind && LanguageKeywords.AccessibilityModifiers.Contains(nodes[currentIndex - 1].Text))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (startsWithI && canGoAhead && new[] { ClassificationTypeNames.ParameterName, ClassificationTypeNames.FieldName }.Contains(nodes[currentIndex + 1].ClassificationType))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (startsWithI && canGoTwoAhead && nodes[currentIndex + 1].Text == ")" && nodes[currentIndex + 2].Text != "{")
        {
            if (currentIndex >= 2)
            {
                var suspectedId = nodes[currentIndex - 2].Id;
                var suspected = _Output.First(x => x.Id == suspectedId);

                if (nodes[currentIndex - 1].Text == "." && new[] { NodeColors.LocalName, NodeColors.ParameterName }.Contains(suspected.Colour))
                    return DetectionStatus.NotDetected();
            }

            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else
        {
            return DetectionStatus.NotDetected();
        }
    }

    private DetectionStatus IsMethod(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        // [InlineData("0001.txt")]
        // var a = list[test()];
        if (currentIndex > 1 && nodes[currentIndex - 1].Text == "[")
        {
            var identifier = nodes[currentIndex - 2].ClassificationType;

            var hasIdentifierBefore = identifier == ClassificationTypeNames.Identifier ||
                identifier == ClassificationTypeNames.LocalName;

            if (!hasIdentifierBefore)
                return DetectionStatus.NotDetected();
        }

        if (!_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            var availableKeywords = new List<string>(LanguageKeywords.AccessibilityModifiers)
            {
                "static",
                "virtual",
                "override"
            };

            if (currentIndex > 0 && availableKeywords.Contains(nodes[currentIndex - 1].Text))
                _IsWithinMethod = true;

            if (currentIndex > 1 && availableKeywords.Contains(nodes[currentIndex - 2].Text))
                _IsWithinMethod = true;

            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (_IsUsing && !_IsUsing && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // simple generics
        else if (!_IsNew && currentIndex + 4 < nodes.Count && nodes[currentIndex + 1].Text == "<"
            && nodes[currentIndex + 3].Text == ">" && nodes[currentIndex + 4].Text == "(")
        {
            var validTypes = new[]
            {
                ClassificationTypeNames.ClassName, ClassificationTypeNames.StructName,
                ClassificationTypeNames.RecordClassName, ClassificationTypeNames.RecordStructName,
                ClassificationTypeNames.Identifier
            };

            return validTypes.Contains(nodes[currentIndex + 2].ClassificationType) ?
                DetectionStatus.DetectedAndDontSkipPostProcessing() :
                DetectionStatus.NotDetected();
        }
        // db.NotifyHandler = OnNotify;
        else if (currentIndex > 0 &&
            currentIndex + 1 < nodes.Count &&
            nodes[currentIndex - 1].Text == "=" && nodes[currentIndex + 1].Text == ";" &&
            nodes[currentIndex].Text.StartsWith("On"))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else
        {
            return DetectionStatus.NotDetected();
        }
    }
    private DetectionStatus IsClassOrStruct(int currentIndex, List<Node> nodes)
    {
        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        var node = nodes[currentIndex];
        bool isPopularClassOrStruct = false;

        if (ThereIsThisInTheChainBefore(currentIndex, nodes))
        {
            return DetectionStatus.NotDetected();
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == ":")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "(")
        {
            _IsNew = false;
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoAhead && nodes[currentIndex + 1].Text == "{")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (_IsNew && canGoAhead &&
            nodes[currentIndex + 1].ClassificationType == ClassificationTypeNames.Punctuation &&
            nodes[currentIndex + 1].Text == "(")
        {
            _IsNew = false;
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (SeemsLikePropertyUsage(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // be careful, if you remove those parenthesis around that assignment, then it'll change its behaviour
        else if ((isPopularClassOrStruct = IsPopularClass(node.Text) || IsPopularStruct(node.Text)) && !canGoBehind)
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (isPopularClassOrStruct && canGoBehind && nodes[currentIndex - 1].Text != ".")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == "<")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoBehind && nodes[currentIndex - 1].Text == "[")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // new DictionaryList<int, int>();
        else if (_IsNew && canGoAhead && nodes[currentIndex + 1].Text == "<")
        {
            _IsNew = false;
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // public void Test(Array<int> a)
        else if (canGoAhead && nodes[currentIndex + 1].Text == "<")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (SeemsLikeMethodParameter(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (RightSideOfAssignmentHasTheSameNameAfterNew(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (currentIndex >= 2 && nodes.Count >= 3 && nodes[currentIndex - 1].ClassificationType == ClassificationTypeNames.Punctuation &&
            nodes[currentIndex - 1].Text == "(" && nodes[currentIndex - 2].Text == "foreach")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (IsParameterWithAttribute(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // cast
        else if (SeemsLikeCast(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoBehind && canGoAhead &&
            LanguageKeywords.AccessibilityModifiers.Contains(nodes[currentIndex - 1].Text) &&
            new[] { ClassificationTypeNames.FieldName, ClassificationTypeNames.ConstantName, ClassificationTypeNames.PropertyName }
            .Contains(nodes[currentIndex + 1].ClassificationType))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoBehind && canGoAhead &&
            LanguageKeywords.AccessibilityModifiers.Contains(nodes[currentIndex - 1].Text) &&
            nodes[currentIndex + 1].Text == "[")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // WebResponse re;
        else if (nodes.Count > currentIndex + 2 &&
            new[] { ClassificationTypeNames.LocalName }.Contains(nodes[currentIndex + 1].ClassificationType) &&
            new[] { ";", "=" }.Contains(nodes[currentIndex + 2].Text))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        // typeof(Test)
        else if (canGoAhead &&
            currentIndex > 1 &&
            nodes[currentIndex + 1].Text == ")" &&
            nodes[currentIndex - 1].Text == "(" &&
            nodes[currentIndex - 2].ClassificationType == ClassificationTypeNames.Keyword &&
            nodes[currentIndex - 2].Text == "typeof")
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }

        return DetectionStatus.NotDetected();
    }

    private DetectionStatus IsPropertyOrField(int currentIndex, List<Node> nodes)
    {
        var node = nodes[currentIndex];

        var canGoAhead = nodes.CanGoAhead(currentIndex);
        var canGoBehind = nodes.CanGoBehind(currentIndex);

        var classifiers = new[]
        {
            ClassificationTypeNames.LocalName, ClassificationTypeNames.PropertyName, ClassificationTypeNames.FieldName,
            ClassificationTypeNames.ConstantName, ClassificationTypeNames.ParameterName
        };

        if (ThereIsThisInTheChainBefore(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (SeemsLikePropertyUsage(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (!_IsNew && ThereIsMethodCallAhead(currentIndex, nodes) &&
            IdentifierFirstCharCaseSeemsLikeVariable(nodes[currentIndex].Text))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (currentIndex >= 2 && nodes[currentIndex - 1].Text == "." && classifiers.Contains(nodes[currentIndex - 2].ClassificationType))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (canGoAhead && currentIndex >= 2
            && nodes[currentIndex + 1].ClassificationType == ClassificationTypeNames.Operator
            && nodes[currentIndex + 1].Text != "."
            && nodes[currentIndex - 1].Text == "("
            && nodes[currentIndex - 2].Text == "if")
        {
            AddVariable(node.Text);
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }
        else if (IsOnInitializationList(currentIndex, nodes))
        {
            return DetectionStatus.DetectedAndDontSkipPostProcessing();
        }

        return DetectionStatus.NotDetected();
    }
}
