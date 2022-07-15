# Change Log
All notable changes to this project since version 1.0.14 will be documented in this file.

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
