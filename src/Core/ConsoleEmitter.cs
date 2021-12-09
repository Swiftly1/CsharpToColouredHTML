using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Classification;

namespace Core
{
    public class ConsoleEmitter : IEmitter
    {
        public void EmitNode(ClassifiedSpan current, SourceText srcText)
        {
            var txt = srcText.ToString(current.TextSpan);

            if (current.ClassificationType == ClassificationTypeNames.ClassName)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (current.ClassificationType == ClassificationTypeNames.NamespaceName)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (current.ClassificationType == ClassificationTypeNames.Identifier)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (current.ClassificationType == ClassificationTypeNames.Keyword)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (current.ClassificationType == ClassificationTypeNames.StringLiteral)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else if (current.ClassificationType == ClassificationTypeNames.LocalName)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else if (current.ClassificationType == ClassificationTypeNames.MethodName)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (current.ClassificationType == ClassificationTypeNames.Punctuation)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (current.ClassificationType == ClassificationTypeNames.Operator)
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (current.ClassificationType == ClassificationTypeNames.ControlKeyword)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
            }
            else
            {
                Console.ResetColor();
            }

            Console.Write(txt);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ResetColor();
        }

        public void EmitText(string s)
        {
            Console.Write(s);
        }
    }
}
