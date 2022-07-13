Here's the default CSS which may help you create your own colors scheme

:warning: Although there is one quirk: the `color` of `background` and `code_column` class is not constant / static. When optimizations are enabled, then the colour of `background`/`code_column` has the value of the most commonly occuring colour in the code. Additionally only used colours in given code fragment are present in generated CSS.

Very often the `#dfdfdf` is the desired value for those, but there are code fragments where colors more blue-ish are the most common

Default CSS with line numbers:

	.background {
	  font-family: monaco, Consolas, LucidaConsole, monospace;
	  background-color: #1e1e1e;
	  overflow: scroll;
	}
	.numeric {
	  color: #b5cea8;
	}
	.method {
	  color: #dcdcaa;
	}
	.class {
	  color: #4ec9b0;
	}
	.keyword {
	  color: #569cd6;
	}
	.string {
	  color: #ce9178;
	}
	.interface {
	  color: #b8d7a3;
	}
	.enumName {
	  color: #b8d7a3;
	}
	.numericLiteral {
	  color: #b8d7a3;
	}
	.recordStruct {
	  color: #b8d7a3;
	}
	.typeParam {
	  color: #b8d7a3;
	}
	.extension {
	  color: #b8d7a3;
	}
	.control {
	  color: #c586c0;
	}
	.internalError {
	  color: #ff0d0d;
	}
	.comment {
	  color: #6a9955;
	}
	.preprocessor {
	  color: #808080;
	}
	.preprocessorText {
	  color: #a4a4a4;
	}
	.struct {
	  color: #86c691;
	}
	.namespace {
	  color: #dfdfdf;
	}
	.enumMember {
	  color: #dfdfdf;
	}
	.identifier {
	  color: #dfdfdf;
	}
	.punctuation {
	  color: #dfdfdf;
	}
	.operator {
	  color: #dfdfdf;
	}
	.propertyName {
	  color: #dfdfdf;
	}
	.fieldName {
	  color: #dfdfdf;
	}
	.labelName {
	  color: #dfdfdf;
	}
	.operator_overloaded {
	  color: #dfdfdf;
	}
	.constant {
	  color: #dfdfdf;
	}
	.localName {
	  color: #9cdcfe;
	}
	.parameter {
	  color: #9cdcfe;
	}
	table {
	  white-space: pre;
	}
	.line_no::before {
	  content: attr(line_no);
	  color: white;
	}
	.code_column {
	  padding-left: 5px;
	}


Default CSS without line numbers:

		.background {
	  font-family: monaco, Consolas, LucidaConsole, monospace;
	  background-color: #1e1e1e;
	  overflow: scroll;
	}
	.numeric {
	  color: #b5cea8;
	}
	.method {
	  color: #dcdcaa;
	}
	.class {
	  color: #4ec9b0;
	}
	.keyword {
	  color: #569cd6;
	}
	.string {
	  color: #ce9178;
	}
	.interface {
	  color: #b8d7a3;
	}
	.enumName {
	  color: #b8d7a3;
	}
	.numericLiteral {
	  color: #b8d7a3;
	}
	.recordStruct {
	  color: #b8d7a3;
	}
	.typeParam {
	  color: #b8d7a3;
	}
	.extension {
	  color: #b8d7a3;
	}
	.control {
	  color: #c586c0;
	}
	.internalError {
	  color: #ff0d0d;
	}
	.comment {
	  color: #6a9955;
	}
	.preprocessor {
	  color: #808080;
	}
	.preprocessorText {
	  color: #a4a4a4;
	}
	.struct {
	  color: #86c691;
	}
	.namespace {
	  color: #dfdfdf;
	}
	.enumMember {
	  color: #dfdfdf;
	}
	.identifier {
	  color: #dfdfdf;
	}
	.punctuation {
	  color: #dfdfdf;
	}
	.operator {
	  color: #dfdfdf;
	}
	.propertyName {
	  color: #dfdfdf;
	}
	.fieldName {
	  color: #dfdfdf;
	}
	.labelName {
	  color: #dfdfdf;
	}
	.operator_overloaded {
	  color: #dfdfdf;
	}
	.constant {
	  color: #dfdfdf;
	}
	.localName {
	  color: #9cdcfe;
	}
	.parameter {
	  color: #9cdcfe;
	}
	
