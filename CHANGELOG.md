# Change Log
All notable changes to this project since version 1.0.14 will be documented in this file.

## [1.0.45] - 20.12.2024
**Description:**
.NET version was increased to .NET 9

Additional heuristics improvements were incorporated.

Refactored codebase a little bit and you will need to adjust namespaces.
e.g Add such using: `using CsharpToColouredHTML.Core.Emitters.HTML;`

Removed workaround introduced in this commit: https://github.com/Swiftly1/CsharpToColouredHTML/commit/81d467d13fc0518e14f8077d47cf8de2d67fecf9

## [1.0.44] - 24.03.2024
**Description:**
Hotfix - https://github.com/Swiftly1/CsharpToColouredHTML/issues/24

## [1.0.43] - 24.03.2024
**Description:**
Fix - https://github.com/Swiftly1/CsharpToColouredHTML/issues/24

Upgrade of Microsoft.CodeAnalysis.CSharp libs

## [1.0.42] - 17.02.2024
**Description:**
Added support for raw strings (""") and fixed some incorrect cases like "something.Property = 5" where Property was marked as a class.
Upgrade of Microsoft.CodeAnalysis.CSharp libs

## [1.0.41] - 02.09.2023
**Description:**
Improved heuristics e.g typeof(), array ([]) as function arg and things incorrectly marked as a classes.
Upgrade of Microsoft.CodeAnalysis.CSharp libs

## [1.0.40] - 01.05.2023
**Description:**
Improved heuristics e.g identifiers like "OnEvent" are detected as a method and other messy class/property colouring was improved.

## [1.0.39] - 18.03.2023
**Description:**
Improved heuristics e.g object initializer syntax and consistency betweeen PropertyName/LocalName. 

## [1.0.38] - 31.01.2023
**Description:**
Two new features added as requested by @kwalkerxxi

Lines highlighting - https://github.com/Swiftly1/CsharpToColouredHTML/issues/18

An ability to allow users to provide their own postprocessing - https://github.com/Swiftly1/CsharpToColouredHTML/issues/17

## [1.0.37] - 27.12.2022
**Description:**
Improved heuristics for using and cases where two things are marked as class next to each other e.g `Thing.SubThing`

## [1.0.36] - 08.12.2022
**Description:**
Improved heuristics e.g this keyword handling

## [1.0.35] - 28.11.2022
**Description:**
Improved heuristics, fixed colour for extension methods and fixed empty input handling

## [1.0.34] - 21.11.2022
**Description:**
Converting new line endings of input code to the new line endings used by OS (Environment.NewLine)

## [1.0.33] - 18.11.2022
**Description:**
Update to .NET 7 and increasing version of Microsoft.CodeAnalysis.CSharp libraries to 4.4.0

## [1.0.32] - 11.11.2022
**Description:**
Improved heuristics for code fragments where namespace names are typed directly instead of using Usings,
also improvements for typeof and for casts like `var b = (Namespace.Type)a;`.

I've partly fought failed heuristics where two classes are detected in one chain like `localVariable.ClassA.ClassB.MethodCall()`

## [1.0.31] - 10.11.2022
**Description:**
Improved heuristics for ASP code fragments, for foreach loop, for parameters with attributes [...] and other.

## [1.0.30] - 09.11.2022
**Description:**
Improved heuristics

## [1.0.29] - 30.10.2022
**Description:**
Improved heuristics

## [1.0.28] - 03.10.2022
**Description:**
Improved heuristics and small refactor: tests and input files rename

## [1.0.27] - 17.09.2022
**Description:**
Improved heuristics 

## [1.0.26] - 11.09.2022
**Description:**
Stopped using obsolete method `Classifier.GetClassifiedSpans` and moved to `Classifier.GetClassifiedSpansAsync`
& upgraded `Microsoft.CodeAnalysis.CSharp.Workspaces` from `4.2.0` to `4.3.0`

## [1.0.25] - 10.09.2022
**Description:** Refactor

**Breaking change** was introduced for people writing their own `IEmitter`

The return type was changed from `void` to `string` and the parameter type of `Emit` method was changed from `List<Node>` to `List<NodeWithDetails>`.
The list of `NodeWithDetails` is already processed list of Nodes, so the heuristics are applied.
Additionally `IEmitter` no longer exposes property `Text` because now it should return text directly from `Emit` method.

The refactor was mostly about splitting generating HTML and generating list of nodes with heuristics applied,
so if somebody wants to write his own Emitter,
then she/he will have access to the nodes with colors/heuristics applied.

## [1.0.24] - 20.08.2022
**Description:**
Improved heuristics for cases like `public void Test(Array<int> a)` or `EqualityComparer<T1>.Default.GetHashCode(P1)`

Increased Iframe window by 40px in order to get rid of scroll bar for 1 liners

## [1.0.23] - 14.08.2022
**Description:**
Fix for unclosed iframe tag

Support for ExcludedCode (compiler directive) and EventName

## [1.0.22] - 15.07.2022
**Description:**
Support for iframe.

Iframe is used to allow users just to copy-paste HTML code onto their website reliably.
Without that very often site's native CSS messes with the generated one and the result is terrible.

## [1.0.21] - 14.07.2022
**Description:**
Support for delegate and improved heuristics

## [1.0.20] - 13.07.2022
**Description:**
Improved heuristics + New Line handling

## [1.0.19] - 13.07.2022
**Description:**
Improvements with CSS generation:
Since now when optimizations are enabled, then only used colours will be present in generated CSS.

## [1.0.18] - 07.07.2022
**Description:**
Hotfix

## [1.0.17] - 07.07.2022
**Description:**
Improved CSS customization by adding way more CSS Colours
e.g "White" CSS class was split into "ConstantName", "ExtensionMethodName", "LabelName" and many more.

Improved optimizer introduced in 1.0.15 by not limiting it to just "White" CSS class.

Improved heuristics:

1) `new Test().TextSpan;` doesn't think that TextSpan is struct in this context

2) `if (node.ClassificationType == ClassificationTypeNames.ClassName)` doesn't think that first `node` is a class


## [1.0.16] - 26.06.2022
**Description:**
Minor CSS changes

## [1.0.15] - 26.06.2022
**Description:**
This patch was focused mostly on optimizing generated HTML code, refactor to separate data preparation from the code gen, minor default CSS changes and breaking changes to the APIs.
_______________
The size differences between generated HTML on some real world examples are:

| Description | Before | After | Diff   |
|-------------|--------|-------|--------|
| Example 1   | 48205  | 33628 | ~30%   |
| Example 2   | 31626  | 21015 | ~33.5% |
| Example 3   | 3592   | 2593  | ~27.8% |

Of course the difference will vary and may be not as this big on very small code fragments.
_______________
Because the amount of config options increased I decided to introduce `HTMLEmitterSettings` object which is used to configure `HTMLEmitter`.

Here are two examples of how to use the lib again. The default usage and the more specific one which modifies defaults.
```csharp
var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter());
```
```csharp
var settings = new HTMLEmitterSettings().DisableLineNumbers().DisableOptimizations().UseCustomCSS("css");
var html = new CsharpColourer().ProcessSourceCode(code, new HTMLEmitter(settings));
```

**Changes Details:**
* Improved generation of HTML, now we avoid creating unnecessary spans by merging spans that have the same colour.
For example this is the fragment of previously generated HTML for this code `public sealed override ref int Q(...`

    ```html
	<tr>
		<td class="line_no" line_no="0"></td>
		<td class="code_column">
			<span class="keyword">public</span>
			<span class="keyword">sealed</span>
			<span class="keyword">override</span>
			<span class="keyword">ref</span>
			<span class="keyword">int</span>
			<span class="method">Q</span>
			<span class="white">(</span>
			...
		</td>
	</tr>
	```
    and this the HTML code with improved code gen
	```html
	<tr>
		<td class="line_no" line_no="0"></td>
		<td class="code_column">
			<span class="keyword">public sealed override ref int</span>
			<span class="method">Q</span>
			<span class="white">(</span>
            ...
		</td>
	</tr>
	```
	
* Removing `<span>` and placing content directly into the HTML for the most common colour nodes (currently works only for "white" colour, but very often that's actually the most common one)

    Let's consider this code: `if (A)`
    
    That's the previously generated code:
    ```html
    <tr>
    	<td class="line_no" line_no="0"></td>
    	<td class="code_column">
    		<span class="control">if</span>
    		<span class="white">(</span>
    		<span class="white">A</span>
    		<span class="white">)</span>
    	</td>
    </tr>
    ```
    The new one is way shorter:
    ```html
	<tr>
		<td class="line_no" line_no="0"></td>
		<td class="code_column">
			<span class="control">if</span> (A)
		</td>
	</tr>
	```
