using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;

namespace Core
{
    public interface IEmitter
    {
        public void EmitNode(ClassifiedSpan current, SourceText srcText);

        public void EmitText(string s);
    }
}