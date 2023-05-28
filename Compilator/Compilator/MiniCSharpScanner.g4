lexer grammar MiniCSharpScanner;

@lexer::members {
    public override void NotifyListeners(LexerNoViableAltException e) {
        this.ErrorListenerDispatch.SyntaxError(this.ErrorOutput, (IRecognizer) this, 0, TokenStartLine, 
        this.TokenStartColumn,"token invalido: '" + 
        this.GetErrorDisplay(this.EmitEOF().InputStream.GetText(Interval.Of(this.TokenStartCharIndex,this.InputStream.Index)))  
        + "'", (RecognitionException) e);
    }
 }

COMMENT : '//' ~[\r\n]* -> skip;
BLOCKCOMMENT : '/*' ( BLOCKCOMMENT | ~[*/] | '/' ~'*' )* '*/' -> skip;
WS  :   [ \t\n\r]+ -> skip;

// Reserved words
CLASS : 'class';
USING : 'using';
VOID : 'void';
IF : 'if';
ELSE : 'else';
FOR : 'for';
WHILE : 'while';
BREAK : 'break';
RETURN : 'return';
READ : 'read';
WRITE : 'write';
NEW : 'new';

// Reserved symbols
LBRACE : '{';
RBRACE : '}';
LPARENT : '(';
RPARENT : ')';
LBRACK : '[';
RBRACK : ']';
SEMICOLON : ';';
COMMA : ',';
DOT : '.';

// Reserved operators
ASSIGN : '=';
INC : '++';
DEC : '--';
OR : '||';
AND : '&&';
EQUAL : '==';
NOTEQUAL : '!=';
GT : '>';
GE : '>=';
LT : '<';
LE : '<=';
ADD : '+';
SUB : '-';
MUL : '*';
DIV : '/';
MOD : '%'; 

//Boolean operators
TrueCONST : 'true';
FalseCONST : 'false';

fragment DIGIT : [0-9];
fragment LETTER : [a-z]|[A-Z];
fragment EXPRESION : LETTER+;
fragment LCOMMENT : '/*';
fragment RCOMMENT : '*/';

DOUBLECONST : DIGIT+ '.' DIGIT+ ;
INTCONST : DIGIT+;
ID : LETTER  (LETTER | DIGIT)*;
STRINGCONST : '"' .*? '"';
CHARCONST : '\'' [a-zA-Z_0-9] '\'' ;
