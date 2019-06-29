# Language Processing (LP)

Koromo Copy에는 언어처리에 도움을 주는 강력한 도구들이 포함되어있습니다.

## LPKor

`한타 <-> 영타` 상호변환을 지원하는 클래스입니다.

```
dc-koromo@koromo-copy:~$ lp -kor-to-eng "오징어 육개장"
dhwlddj dbrrowkd
dc-koromo@koromo-copy:~$ lp -eng-to-kor "dhwlddj dbrrowkd"
오징어 육개장
```

| 함수 이름 | 설명 |
|-----|-----|
|IsHangul|어떤 글자가 한글 알파벳인지 확인합니다.|
|Disassembly|한글 알파벳을 적절한 영타로 변환합니다.|
|Assembly|영타로 쓰여진 한글을 한글알파벳으로 변환합니다.|

각 함수에 접미사로 `3`을 붙이면 `세벌식 최종`의 `한타`를 사용할 수 있습니다.

## Scanner Generator

`ESRCAL`을 분석하기 위해 필요한 기능을 제공하는 스캐너 제너레이터입니다.
정규 표현식 문법 중 `[~]`, `[^~]`, `+`, `?`, `*`, `(~)`, `|`을 제공합니다.
`.`는 현재 구현되어 있지 않습니다.
또한 ASCII Code만 사용이 가능합니다.

``` cs
var sg = new ScannerGenerator();

sg.PushRule("", @"[\r\n ]");
sg.PushRule("op_open", @"\(");
sg.PushRule("op_close", @"\)");
sg.PushRule("pp_open", @"\[");
sg.PushRule("pp_close", @"\]");
sg.PushRule("equal", @"\=");
sg.PushRule("plus", @"\+");
sg.PushRule("minus", @"\-");
sg.PushRule("multiple", @"\*");
sg.PushRule("divide", @"\/");
sg.PushRule("id", @"[_$a-zA-Z][_$a-zA-Z0-9]*");
sg.PushRule("num", @"[0-9]+(\.[0-9]+)?([Ee][\+\-]?[0-9]+)?");

sg.Generate();

var ss = sg.CreateScannerInstance();

ss.AllocateTarget(line.Trim());

while (ss.Valid())
{
    var tk = ss.Next();
    if (ss.Error())
        throw new Exception("[COMPILER] Tokenize error! '" + tk + "'");
    insert(tk.Item1, tk.Item2, ll, tk.Item4);
}
```

Scanner Generator 클래스

| 함수 이름 | 설명 |
|-----|-----|
|PushRule(token, pattern)|규칙을 삽입합니다. 먼저 Push한 규칙이 우선순위가 높습니다.|
|Generate|등록된 규칙들을 기반으로 Merged DFA를 생성합니다.|
|CreateScannerInstance|스캐너 인스턴스를 생성합니다. 반드시 Generate함수를 호출한 다음에 호출해야합니다.|

Scanner 클래스

| 함수 이름 | 설명 |
|-----|-----|
|AllocateTarget|스캐너를 초기화하고, 분석할 문자열을 등록합니다.|
|Valid|현재 스캐너의 상태가 유효한지 확인합니다.|
|Next|다음 토큰을 가져옵니다.|
|Lookahead|다음에 나올 토큰을 가져옵니다.|
|Error|스캐너 내부에서 오류가 발생했는지 확인합니다.|

## Parser Generator

`SLR`, `LR(1)`, `LALR` 파서를 생성할 수 있는 강력한 파서 제너레이터입니다.

다음은 간단한 수식 `NUM`, `+`, `-`, `*`, `-`, `(~)` 규칙을 승인하는 파서를 생성하는 파서 제너레이터 코드입니다.

``` cs
var gen = new ParserGenerator();

// Non-Terminals
var expr = gen.CreateNewProduction("expr", false);

// Terminals
var num = gen.CreateNewProduction("num");
var plus = gen.CreateNewProduction("plus");
var minus = gen.CreateNewProduction("minus");
var multiple = gen.CreateNewProduction("multiple");
var divide = gen.CreateNewProduction("divide");
var op_open = gen.CreateNewProduction("op_open");
var op_close = gen.CreateNewProduction("op_close");

expr |= num + ParserAction.Create(x => x.UserContents = double.Parse(x.Contents));
expr |= expr + plus + expr + ParserAction.Create(x => x.UserContents = (double)x.Childs[0].UserContents + (double)x.Childs[2].UserContents);
expr |= expr + minus + expr + ParserAction.Create(x => x.UserContents = (double)x.Childs[0].UserContents - (double)x.Childs[2].UserContents);
expr |= expr + multiple + expr + ParserAction.Create(x => x.UserContents = (double)x.Childs[0].UserContents * (double)x.Childs[2].UserContents);
expr |= expr + divide + expr + ParserAction.Create(x => x.UserContents = (double)x.Childs[0].UserContents / (double)x.Childs[2].UserContents);
expr |= minus + expr + ParserAction.Create(x => x.UserContents = -(double)x.Childs[1].UserContents);
expr |= op_open + expr + op_close + ParserAction.Create(x => x.UserContents = x.Childs[1].UserContents);

// right associativity, -
gen.PushConflictSolver(false, new Tuple<ParserProduction, int>(expr, 5));
// left associativity, *, /
gen.PushConflictSolver(true, multiple, divide);
// left associativity, +, -
gen.PushConflictSolver(true, plus, minus);

try
{
    gen.PushStarts(expr);
    gen.PrintProductionRules();
    gen.GenerateLALR();
    gen.PrintStates();
    gen.PrintTable();
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

Console.WriteLine(gen.GlobalPrinter.ToString());

var pp = gen.CreateShiftReduceParserInstance();

Action<string, string, int, int> insert = (string x, string y, int a, int b) =>
{
    pp.Insert(x, y);
    if (pp.Error()) throw new Exception($"[COMPILER] Parser error! L:{a}, C:{b}");
    while (pp.Reduce())
    {
        var l = pp.LatestReduce();
        Console.Write(l.Production.PadLeft(8) + " => ");
        Console.WriteLine(string.Join(" ", l.Childs.Select(z => z.Production)));
        Console.Write(l.Production.PadLeft(8) + " => ");
        Console.WriteLine(string.Join(" ", l.Childs.Select(z => z.Contents)));
        pp.Insert(x, y);
        if (pp.Error()) throw new Exception($"[COMPILER] Parser error! L:{a}, C:{b}");
    }
};

try
{
    ss.AllocateTarget("5+4-(-4*(2-4)*2)/3+-(-(2*2+3)-2)*(3+1)");

    while (ss.Valid())
    {
        var tk = ss.Next();
        if (ss.Error())
            throw new Exception("[COMPILER] Tokenize error! '" + tk + "'");
        insert(tk.Item1, tk.Item2, ll, tk.Item4);
    }
    if (pp.Error()) throw new Exception();
    insert("$", "$", -1, -1);

    var tree = pp.Tree;

    Console.WriteLine($"Query: {lines[0]}");
    Console.WriteLine($"Answer: {(double)(tree.root.UserContents)}");
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
```

```
+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+
|          |      num |     plus |    minus | multiple |   divide |  op_open | op_close |        $ |     expr |
+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+
|       0  |       s2 |          |       s3 |          |          |       s4 |          |          |        1 |
|       1  |          |       s5 |       s6 |       s7 |       s8 |          |          |      acc |          |
|       2  |          |       r1 |       r1 |       r1 |       r1 |          |       r1 |       r1 |          |
|       3  |       s2 |          |       s3 |          |          |       s4 |          |          |        9 |
|       4  |       s2 |          |       s3 |          |          |       s4 |          |          |       10 |
|       5  |       s2 |          |       s3 |          |          |       s4 |          |          |       11 |
|       6  |       s2 |          |       s3 |          |          |       s4 |          |          |       12 |
|       7  |       s2 |          |       s3 |          |          |       s4 |          |          |       13 |
|       8  |       s2 |          |       s3 |          |          |       s4 |          |          |       14 |
|       9  |          |       r6 |       r6 |       r6 |       r6 |          |       r6 |       r6 |          |
|      10  |          |       s5 |       s6 |       s7 |       s8 |          |      s15 |          |          |
|      11  |          |       r2 |       r2 |       s7 |       s8 |          |       r2 |       r2 |          |
|      12  |          |       r3 |       r3 |       s7 |       s8 |          |       r3 |       r3 |          |
|      13  |          |       r4 |       r4 |       r4 |       r4 |          |       r4 |       r4 |          |
|      14  |          |       r5 |       r5 |       r5 |       r5 |          |       r5 |       r5 |          |
|      15  |          |       r7 |       r7 |       r7 |       r7 |          |       r7 |       r7 |          |
+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+
```

Parser Generator 클래스

| 함수 이름 | 설명 |
|-----|-----|
|CreateNewProduction(name, is-terminal)|새로운 `Production`을 생성합니다. `is-terminal`이 `true`라면 터미널 `Production`이 생성됩니다.|
|PushConflictSolver|Shift-Reduce Conflict, Reduce-Reduce Conflict를 해결할 규칙을 삽입합니다. 먼저 삽입된 규칙이 높은 우선순위를 가집니다.|
|PushStarts|규칙 분석을 시작할 `Start Symbol`을 설정합니다.|
|GenerateLALR|`LALR` 테이블을 생성합니다. `LR1`, `SLR`도 가능합니다.|
|CreateShiftReduceParserInstance|파서 인스턴스를 생성합니다.|

Parser 클래스

| 함수 이름 | 설명 |
|-----|-----|
|Insert|토큰을 삽입합니다.|
|Reduce|Reduce할 수 있다면 Reduce를 진행합니다.|
|Error|파서 내부에서 오류가 발생했는지 확인합니다.|

If-Else Conflict 해결 예제

``` cs
if_statement |= if + ~ + body;
if_statement |= if + ~ + body + else + body;

// 이러한 규칙은 Non-associtive로 취급됨
gen.PushConflictSolver(true, else_token);
gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(if_statement, 1)); 
```

트리노드 출력

``` cs
public void PrintTree(ParsingTree.ParsingTreeNode node, string indent, bool last)
{
    Console.Console.Instance.Write(indent);
    if (last)
    {
        Console.Console.Instance.Write("+-");
        indent += "  ";
    }
    else
    {
        Console.Console.Instance.Write("|-");
        indent += "| ";
    }

    if (node.Childs.Count == 0)
    {
        Console.Console.Instance.WriteLine(node.Production + " " + node.UserContents);
    }
    else
    {
        Console.Console.Instance.WriteLine(node.Production + " " + node.UserContents);
    }
    for (int i = 0; i < node.Childs.Count; i++)
        PrintTree(node.Childs[i], indent, i == node.Childs.Count - 1);
}
```

```
+-expr 39.6666666666667
  |-expr 3.66666666666667
  | |-expr 9
  | | |-expr 5
  | | | +-num
  | | |-plus
  | | +-expr 4
  | |   +-num
  | |-minus
  | +-expr 5.33333333333333
  |   |-expr 16
  |   | |-op_open
  |   | |-expr 16
  |   | | |-expr 8
  |   | | | |-expr -4
  |   | | | | |-minus
  |   | | | | +-expr 4
  |   | | | |   +-num
  |   | | | |-multiple
  |   | | | +-expr -2
  |   | | |   |-op_open
  |   | | |   |-expr -2
  |   | | |   | |-expr 2
  |   | | |   | | +-num
  |   | | |   | |-minus
  |   | | |   | +-expr 4
  |   | | |   |   +-num
  |   | | |   +-op_close
  |   | | |-multiple
  |   | | +-expr 2
  |   | |   +-num
  |   | +-op_close
  |   |-divide
  |   +-expr 3
  |     +-num
  |-plus
  +-expr 36
    |-expr 9
    | |-minus
    | +-expr -9
    |   |-op_open
    |   |-expr -9
    |   | |-expr -7
    |   | | |-minus
    |   | | +-expr 7
    |   | |   |-op_open
    |   | |   |-expr 7
    |   | |   | |-expr 4
    |   | |   | | |-expr 2
    |   | |   | | | +-num
    |   | |   | | |-multiple
    |   | |   | | +-expr 2
    |   | |   | |   +-num
    |   | |   | |-plus
    |   | |   | +-expr 3
    |   | |   |   +-num
    |   | |   +-op_close
    |   | |-minus
    |   | +-expr 2
    |   |   +-num
    |   +-op_close
    |-multiple
    +-expr 4
      |-op_open
      |-expr 4
      | |-expr 3
      | | +-num
      | |-plus
      | +-expr 1
      |   +-num
      +-op_close
Query: 5+4-(-4*(2-4)*2)/3+-(-(2*2+3)-2)*(3+1)
Answer: 39.6666666666667
```