using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;

namespace CsharpToColouredHTML.Core.HeuristicsGeneration;

internal partial class HeuristicsGenerator
{
    private void GenerateHeuristics()
    {
        do
        {
            try
            {
                HandleCounters();

                if (HintsAndAlreadyClassifiedNodes())
                    continue;

                if (IsKeyword())
                    continue;

                if (IsInterface())
                    continue;

                if (IsComment())
                    continue;

                if (IsPunctuation())
                    continue;

                if (IsMethod())
                    continue;

                if (IsClassOrStruct())
                    continue;

                if (IsProperty())
                    continue;

                if (ResolveNamespaces())
                    continue;

                if (IsPragma())
                    continue;

                MarkNodeAs(NodeColors.DefaultColour);
            }
            catch
            {
                MarkNodeAs(NodeColors.InternalError);
            }
        } while (MoveNext());
    }

    private bool IsPragma()
    {
        if (CurrentNode.ClassificationType != ClassificationTypeNames.Identifier)
            return false;

        if (TryPeekBehind(out var nodeBehind) && nodeBehind.ClassificationType.EqualsAnyOf(
            ClassificationTypeNames.PreprocessorKeyword, ClassificationTypeNames.PreprocessorText))
        {
            MarkNodeAs(NodeColors.Identifier);
            return true;
        }

        return false;
    }

    private bool ResolveNamespaces()
    {
        if (CurrentNode.ClassificationType != ClassificationTypeNames.Identifier)
            return false;

        if (TryPeekAhead(out var peekedAhead) && peekedAhead.Text != ".")
            return false;

        if (_FoundNamespaces.Contains(CurrentText))
        {
            MarkNodeAs(NodeColors.Namespace);
            return true;
        }

        return false;
    }

    private void HandleCounters()
    {
        if (CurrentText == "if" && TryPeekAhead(out var nextNode) && nextNode.Text == "(")
        {
            _ParenthesisCounter = 0;
            _InsideIfStatement = true;
        }

        if (CurrentText == "new")
        {
            _ParenthesisCounter = 0;
            _InsideNewStatement = true;
        }

        if (CurrentText == "(")
            _ParenthesisCounter++;

        if (CurrentText == ")" && _ParenthesisCounter > 0)
        {
            _ParenthesisCounter--;

            if (_ParenthesisCounter <= 0)
            {
                _InsideIfStatement = false;
                _InsideNewStatement = false;
            }
        }

        if (CurrentText == ";")
        {
            _ParenthesisCounter = 0;
            _InsideIfStatement = false;
            _InsideNewStatement = false;
        }
    }

    private bool IsClassOrStruct()
    {
        var found = false;

        if (!IsValidClassOrStructName(CurrentText))
            return false;

        if (TryPeekBehind(out var peekedBehindNode) && TryPeekBehind(out var peekedBehindNode2, 2) &&
            peekedBehindNode.Text == "." &&
            peekedBehindNode2.Text == ">")
        {
            if (TryPeekAhead(out var peekAhead) && peekAhead.Text == "(")
                MarkNodeAs(NodeColors.Method);
            else
                MarkNodeAs(NodeColors.PropertyName);

            return true;
        }

        // HomeController : Controller
        if (TryPeekBehind(out peekedBehindNode) && peekedBehindNode.Text == ":")
        {
            found = true;
            goto Exit;
        }

        // JsonConvert.SerializeObject(html)
        if (TryPeekAhead(out var peekedAheadNode) && peekedAheadNode.Text == "." &&
            TryPeekBehind(out peekedBehindNode) && peekedBehindNode.Text != ".")
        {
            if (TryPeekAhead(out var peekedMethod1, 2) && TryPeekAhead(out var peekedPunctuation, 3))
            {
                var valid_identifiers = new List<string>
                {
                    ClassificationTypeNames.Identifier,
                    ClassificationTypeNames.MethodName
                };

                if (valid_identifiers.Contains(peekedMethod1.ClassificationType) && peekedPunctuation.Text == "(")
                {
                    found = true;
                    goto Exit;
                }
            }
        }

        // foreach (DateTime ...
        if (TryPeekBehind(out peekedBehindNode) && peekedBehindNode.Text == "(" &&
            TryPeekBehind(out peekedBehindNode2, 2) && peekedBehindNode2.Text == "foreach")
        {
            found = true;
            goto Exit;
        }

        // private readonly Node<T, U> Root = new Node<T, U>(default!, null) { IsRoot = true };
        if (TryPeekAhead(out peekedAheadNode) && peekedAheadNode.Text == "<" &&
            TryPeekBehind(out peekedBehindNode) && peekedBehindNode.Text != ".")
        {
            found = true;
            goto Exit;
        }

        // Callback func
        if (TryPeekAhead(out peekedAheadNode) && peekedAheadNode.ClassificationType.EqualsAnyOf(
            ClassificationTypeNames.ParameterName,
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.FieldName))
        {
            found = true;
            goto Exit;
        }

        // Param[] Params,
        if (TryPeekAhead(out peekedAheadNode) && TryPeekAhead(out var peekedAheadNode2, 2))
        {
            if (peekedAheadNode.Text == "[" && peekedAheadNode2.Text == "]")
            {
                if (TryPeekAhead(out var peekedAheadNode3, 3) &&
                    peekedAheadNode3.ClassificationType.EqualsAnyOf(
                    ClassificationTypeNames.ParameterName,
                    ClassificationTypeNames.LocalName,
                    ClassificationTypeNames.FieldName))
                {
                    found = true;
                    goto Exit;
                }
            }
        }

        if (TryPeekBehind(out peekedBehindNode) && peekedBehindNode.Text == "(" &&
            TryPeekBehind(out peekedBehindNode2, 2) &&
            TryPeekAhead(out peekedAheadNode) && peekedAheadNode.Text == ")")
        {
            var isIdentifierBefore = peekedBehindNode2.ClassificationType.EqualsAnyOf
            (
                ClassificationTypeNames.MethodName,
                ClassificationTypeNames.ClassName,
                ClassificationTypeNames.StructName,
                ClassificationTypeNames.RecordClassName,
                ClassificationTypeNames.RecordStructName
            );

            if (peekedBehindNode2.Text != "if" && !isIdentifierBefore)
            {
                var nodeHasValidClassification = CurrentNode.ClassificationType.EqualsAnyOf
                (
                    ClassificationTypeNames.Identifier,
                    ClassificationTypeNames.ClassName,
                    ClassificationTypeNames.StructName
                );

                if (nodeHasValidClassification)
                {
                    found = true;
                    goto Exit;
                }
            }
        }

        if (TryPeekAhead(out peekedAheadNode) && peekedAheadNode.Text == "?" && TryPeekAhead(out peekedAheadNode2, 2))
        {
            var validClassification = peekedAheadNode2.ClassificationType.EqualsAnyOf
            (
                ClassificationTypeNames.LocalName
            );

            if (validClassification)
            {
                found = true;
                goto Exit;
            }
        }

        if (TryPeekAhead(out peekedAheadNode) && peekedAheadNode.ClassificationType == ClassificationTypeNames.MethodName)
        {
            found = true;
            goto Exit;
        }

        if (TryPeekBehind(out peekedBehindNode) && peekedBehindNode.Text == "[")
        {
            if (TryPeekBehind(out peekedBehindNode2, 2))
            {
                var valid_identifiers = new List<string>
                {
                    ClassificationTypeNames.LocalName,
                    ClassificationTypeNames.PropertyName,
                    ClassificationTypeNames.ConstantName,
                    ClassificationTypeNames.FieldName,
                    ClassificationTypeNames.ParameterName,
                };

                if (!valid_identifiers.Contains(peekedBehindNode2.ClassificationType))
                {
                    found = true;
                    goto Exit;
                }
            }
            else
            {
                found = true;
                goto Exit;
            }
        }

        // catch (Exception ex)
        if (TryPeekBehind(out peekedBehindNode) && peekedBehindNode.Text == "(" &&
            TryPeekBehind(out peekedBehindNode2, 2) && peekedBehindNode2.Text == "catch")
        {
            found = true;
            goto Exit;
        }

        // EqualityComparer<T1>
        // EqualityComparer<T1, T2, T3>
        if (CheckAndMarkGenericParametersChain())
        {
            // return because CheckAndMarkGenericParametersChain already marked them and moved pointer
            return true;
        }

        // var q = (TestQ.Home)h;
        if (TryDetectCastAhead())
            return true;

        if (CheckClassPropertiesUsageChain())
        {
            return true;
        }

        Exit:
        if (found)
        {
            var colour = ResolveName(CurrentText);
            MarkNodeAs(colour);
            return true;
        }

        return false;
    }

    private bool IsProperty()
    {
        var validVariableIdentifiers = new List<string>
        {
            ClassificationTypeNames.LocalName,
            ClassificationTypeNames.PropertyName,
            ClassificationTypeNames.ConstantName,
            ClassificationTypeNames.FieldName,
            ClassificationTypeNames.ParameterName,
            ClassificationTypeNames.StructName,
            ClassificationTypeNames.ClassName,
            ClassificationTypeNames.RecordClassName,
            ClassificationTypeNames.RecordStructName,
        };

        if (TryPeekBehind(out var peekedOperator) && peekedOperator.Text == "." &&
            TryPeekBehind(out var peekedVariable, 2) &&
            TryPeekAhead(out var peekedAhead) && peekedAhead.Text != "(")
        {
            if (validVariableIdentifiers.Contains(peekedVariable.ClassificationType))
            {
                MarkNodeAs(NodeColors.PropertyName);
                return true;
            }
        }

        if (TryPeekBehind(out peekedOperator) && peekedOperator.Text == "." &&
            TryPeekBehind(out var peekedOperator2, 2) && peekedOperator2.Text == "?" &&
            TryPeekBehind(out peekedVariable, 3) &&
            TryPeekAhead(out peekedAhead) && peekedAhead.Text != "(")
        {
            if (validVariableIdentifiers.Contains(peekedVariable.ClassificationType))
            {
                MarkNodeAs(NodeColors.PropertyName);
                return true;
            }
        }

        // var id = home.Areas?.FirstOrDefault()?.Id;
        if (TryPeekBehind(out peekedOperator) && peekedOperator.Text == "." &&
            TryPeekBehind(out peekedOperator2, 2) && peekedOperator2.Text == "?" &&
            TryPeekBehind(out var peekedParenthesis, 3) && peekedParenthesis.Text == ")" &&
            TryPeekAhead(out peekedAhead) && peekedAhead.Text != "(")
        {
            MarkNodeAs(NodeColors.PropertyName);
            return true;
        }

        if (TryPeekBehind(out peekedOperator) && peekedOperator.Text == "." &&
            TryPeekBehind(out var peekedIndexer, 2) && peekedIndexer.Text == "]")
        {
            MarkNodeAs(NodeColors.PropertyName);
            return true;
        }

        if (TryPeekAhead(out peekedIndexer) && peekedIndexer.Text == "[")
        {
            MarkNodeAs(NodeColors.PropertyName);
            return true;
        }

        if (TryPeekBehind(out peekedOperator) && peekedOperator.Text.EqualsAnyOf("{", ",") &&
            TryPeekAhead(out var peekedNext) && peekedNext.Text.Contains("="))
        {
            MarkNodeLocalNameOrProperty(CurrentNode);
            return true;
        }

        if (TryPeekBehind(out peekedOperator) && peekedOperator.Text == "." &&
            TryPeekBehind(out peekedParenthesis, 2) && peekedParenthesis.Text == ")")
        {
            MarkNodeAs(NodeColors.PropertyName);
            return true;
        }

        if (_InsideIfStatement && CurrentNode.ClassificationType == ClassificationTypeNames.Identifier)
        {
            if (TryPeekAhead(out var nextNode) && nextNode.Text == ")")
            {
                MarkNodeAs(NodeColors.PropertyName);
                _FoundPropertiesOrFields.Add(CurrentText);
            }
            else
            {
                if (IdentifierFirstCharCaseSeemsLikeVariable(CurrentText))
                {
                    MarkNodeAs(NodeColors.LocalName);
                    _FoundLocalNames.Add(CurrentText);
                }
                else
                {
                    MarkNodeAs(NodeColors.PropertyName);
                    _FoundPropertiesOrFields.Add(CurrentText);
                }
            }
            return true;
        }

        if (_InsideNewStatement && _ParenthesisCounter > 0 &&
            CurrentNode.ClassificationType == ClassificationTypeNames.Identifier)
        {
            MarkNodeLocalNameOrProperty(CurrentNode);
            return true;
        }

        if (TryPeekBehind(out peekedOperator) && peekedOperator.Text == "." &&
            TryPeekBehind(out var peekedThis, 2) && peekedThis.Text == "this")
        {
            MarkNodeAs(NodeColors.PropertyName);
            return true;
        }

        // return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        if (TryPeekBehind(out peekedOperator) && Operators.Contains(peekedOperator.Text))
        {
            if (TryPeekAhead(out var nodeAhead1) && nodeAhead1.Text.EqualsAnyOf(";", "}"))
            {
                MarkNodeLocalNameOrProperty(CurrentNode);
                return true;
            }
        }

        // return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        if (TryPeekAhead(out peekedOperator) && Operators.Contains(peekedOperator.Text))
        {
            MarkNodeLocalNameOrProperty(CurrentNode);
            return true;
        }

        // public static IEnumerable<(TSource Source, TOut Out)>
        if (TryPeekAhead(out var nodeAhead) && nodeAhead.Text.EqualsAnyOf(",", ")") &&
            CurrentNode.ClassificationType == ClassificationTypeNames.Identifier)
        {
            MarkNodeLocalNameOrProperty(CurrentNode);
            return true;
        }

        if (TryPeekBehind(out var nodeBehind) && nodeBehind.Text == "," &&
            CurrentNode.ClassificationType == ClassificationTypeNames.Identifier)
        {
            MarkNodeLocalNameOrProperty(CurrentNode);
            return true;
        }

        if (TryPeekBehind(out nodeBehind) && nodeBehind.Text == "(" && TryPeekBehind(out var nodeBehind2, 2))
        {
            var valid_identifiers = new List<string>
            {
                ClassificationTypeNames.MethodName
            };

            if (valid_identifiers.Contains(nodeBehind2.ClassificationType))
            {
                MarkNodeLocalNameOrProperty(CurrentNode);
                return true;
            }
        }

        if (IdentifierFirstCharCaseSeemsLikeVariable(CurrentText))
        {
            MarkNodeAs(NodeColors.LocalName);
            return true;
        }

        return false;
    }

    private void MarkNodeLocalNameOrProperty(Node currentNode)
    {
        if (IdentifierFirstCharCaseSeemsLikeVariable(currentNode.Text))
        {
            MarkNodeAs(NodeColors.LocalName);
            _FoundLocalNames.Add(CurrentText);
        }
        else
        {
            MarkNodeAs(NodeColors.PropertyName);
            _FoundPropertiesOrFields.Add(CurrentText);
        }
    }

    private bool IsMethod()
    {
        if (TryPeekAhead(out var peekedNode) && peekedNode.Text == "(")
        {
            if (!CanMoveBehind())
            {
                MarkNodeAs(NodeColors.Method);
                return true;
            }

            if (TryPeekBehind(out var peekBehind))
            {
                if (peekBehind.Text == "new")
                {
                    return false;
                }
                else if (peekBehind.Text.EqualsAnyOf(".", "return", "await", "=", "=>"))
                {
                    var newIsUsed = CheckIfThereIsNewBeforeMethodCall();
                    if (newIsUsed)
                    {
                        return false;
                    }
                    else
                    {
                        MarkNodeAs(NodeColors.Method);
                        return true;
                    }
                }
                else if (peekBehind.Text.EqualsAnyOf(";", "{"))
                {
                    MarkNodeAs(NodeColors.Method);
                    return true;
                }
                else if (CommonKeywordsBeforeTypeName.Contains(peekBehind.Text))
                {
                    MarkNodeAs(NodeColors.Method);
                    return true;
                }
                else if (peekBehind.Text == "[")
                {
                    if (TryPeekBehind(out var peekBehind2, 2))
                    {
                        var valid_identifiers = new List<string>
                        {
                            ClassificationTypeNames.LocalName,
                            ClassificationTypeNames.PropertyName,
                            ClassificationTypeNames.ConstantName,
                            ClassificationTypeNames.FieldName,
                            ClassificationTypeNames.ParameterName,
                        };

                        if (valid_identifiers.Contains(peekBehind2.ClassificationType))
                        {
                            MarkNodeAs(NodeColors.Method);
                            return true;
                        }
                    }
                }
                else if (peekBehind.ClassificationType == ClassificationTypeNames.Identifier)
                {
                    MarkNodeAs(peekBehind, ResolveName(peekBehind.Text));
                    MarkNodeAs(NodeColors.Method);
                    return true;
                }
                else if (peekBehind.ClassificationType.EqualsAnyOf(ClassificationTypeNames.Comment, ClassificationTypeNames.Punctuation))
                {
                    MarkNodeAs(NodeColors.Method);
                    return true;
                }
            }
        }
        else if (TryPeekBehind(out var peekedBehind) && peekedBehind.Text.EqualsAnyOf("=", "+=", "-=") &&
                TryPeekAhead(out var peekAhead) && peekAhead.Text == ";")
        {
            if (SoundsLikeEventOrHandler(CurrentText))
            {
                MarkNodeAs(NodeColors.Method);
                return true;
            }
        }
        else if (TryPeekAhead(out peekAhead) && peekAhead.Text == "<" &&
                 TryPeekBehind(out peekedBehind) && peekedBehind.Text != "new")
        {
            // .WhereOut<TKey, TItem>(dictionary.TryGetValue)
            if (LookAheadWhatIsAfterGenerics(peekAhead, out var nodeAfter) && nodeAfter.Text == "(")
            {
                MarkNodeAs(NodeColors.Method);
                return true;
            }
        }

        return false;
    }

    private bool IsInterface()
    {
        if (!NameLikeInterface(CurrentText))
            return false;

        var isInterface = false;

        if (TryPeekBehind(out var peekedNode0))
        {
            if (peekedNode0.Text.EqualsAnyOf(":", "<"))
                isInterface = true;

            if (CommonKeywordsBeforeTypeName.Contains(peekedNode0.Text))
                isInterface = true;
        }

        if (TryPeekAhead(out var peekedNode1))
        {
            if (peekedNode1.ClassificationType.EqualsAnyOf(ClassificationTypeNames.ParameterName, ClassificationTypeNames.FieldName))
            {
                isInterface = true;
            }
        }

        if (CanMoveAhead(2) && CanMoveBehind(2))
        {
            var next1 = _OriginalNodes[_CurrentIndex + 1];
            var next2 = _OriginalNodes[_CurrentIndex + 2];

            var prev1 = _OriginalNodes[_CurrentIndex - 1];
            var prev2 = _Output[_CurrentIndex - 2];

            if (next1.Text == ")" && next2.Text != "{")
            {
                if (prev1.Text == "." &&
                    prev2
                    .ClassificationType
                    .EqualsAnyOf(ClassificationTypeNames.LocalName, ClassificationTypeNames.ParameterName))
                {
                    return false;
                }
                else
                {
                    isInterface = true;
                }
            }
        }

        if (isInterface)
        {
            MarkNodeAs(NodeColors.Interface);
            return true;
        }

        return false;
    }

    private bool HintsAndAlreadyClassifiedNodes()
    {
        if (_SimpleClassificationToColourMapper.TryGetValue(CurrentNode.ClassificationType, out var simpleColour))
        {
            MarkNodeAs(simpleColour);
            return true;
        }

        if (_Hints.BuiltInTypes.Contains(CurrentNode.Text))
        {
            MarkNodeAs(NodeColors.Keyword);
            return true;
        }

        return false;
    }

    private bool IsPunctuation()
    {
        if (CurrentNode.ClassificationType == ClassificationTypeNames.Punctuation)
        {
            MarkNodeAs(NodeColors.Punctuation);
            return true;
        }

        return false;
    }

    private bool IsKeyword()
    {
        if (CurrentNode.ClassificationType != ClassificationTypeNames.Keyword)
            return false;

        MarkNodeAs(NodeColors.Keyword);

        if (CurrentText == "using")
        {
            TryReadNamespaceChain();
        }
        else if (CurrentText == "new")
        {
            if (TryPeekAhead(out var peekedAhead1) && TryPeekAhead(out var peekedAhead2, 2))
            {
                // TODO: Potentially we can use only second list
                var validIdentifiers1 = new[]
                {
                    ClassificationTypeNames.Identifier,
                    ClassificationTypeNames.ClassName,
                };

                var validIdentifiers2 = new[]
{
                    ClassificationTypeNames.Identifier,
                    ClassificationTypeNames.ClassName,
                    ClassificationTypeNames.NamespaceName,
                };

                if (validIdentifiers1.Contains(peekedAhead1.ClassificationType) &&
                    peekedAhead2.Text.EqualsAnyOf("(", "{", "["))
                {
                    var colour = ResolveName(peekedAhead1.Text, true);
                    MarkNodeAs(peekedAhead1, colour);
                    MoveNext();
                }
                else if (validIdentifiers2.Contains(peekedAhead1.ClassificationType) &&
                         peekedAhead2.Text == ".")
                {
                    TryReadIdentifierChain();
                }
            }
        }
        else if (CurrentText == "typeof")
        {
            if (TryPeekAhead(out var nodeAhead) && nodeAhead.Text == "(")
            {
                MoveNext();
                MarkNodeAs(nodeAhead, NodeColors.Punctuation);
                if (MoveNext())
                {
                    if (!TryConsumeTypeNameAhead())
                        MoveBehind();
                }
            }
        }
        else if (CurrentText == "as")
        {
            if (TryPeekAhead(out var nodeAhead) && nodeAhead.ClassificationType == ClassificationTypeNames.Identifier)
            {
                MoveNext();
                var colour = ResolveName(nodeAhead.Text);
                MarkNodeAs(nodeAhead, colour);
            }
        }
        else if (CommonKeywordsBeforeTypeName.Contains(CurrentText))
        {
            MarkNodeAs(NodeColors.Keyword);
            // private (int NewIndex, TextSpan CurrentTextSpan) HandleMultilineStrings() {}
            if (MoveNext())
            {
                if (TryConsumeTypeNameAhead())
                {
                    return true;
                }
                else
                {
                    MoveBehind();
                    return true;
                }
            }
        }

        return true;
    }

    private bool IsComment()
    {
        if (CurrentNode.ClassificationType.Contains("xml doc comment"))
        {
            MarkNodeAs(NodeColors.Comment);
            return true;
        }

        return false;
    }
}