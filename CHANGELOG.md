# Change Log
All notable changes to this project since version 1.0.14 will be documented in this file.

## [1.0.15] - 26.06.2022
**Description:**
This patch was focused mostly on optimizing generated HTML code, refactor to separate data preparation from the code gen, minor default CSS changes and breaking changes to the APIs.
_______________
The size difference between generated HTML on some real world examples is:

Example1:
Before: 48205
After: 33628
**Diff: ~30%**

Example2:
Before: 31626
After: 21015
**Diff: ~33.5%**

Example3:
Before: 3592
After: 2593
**Diff: ~27.8%**

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
