/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using Koromo_Copy.LP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.JS
{
    /// <summary>
    /// Java Script Parser
    /// </summary>
    public class JSParserGenerator
    {
        static ExtendedShiftReduceParser parser;
        public static ExtendedShiftReduceParser Parser { get { if (parser == null) create_parser(); return parser; } }

        /// <summary>
        /// Create JavaScript Parser
        /// </summary>
        private static void create_parser()
        {
            //parser = new JSParser().Parser;

            var gen = new ParserGenerator();

            // https://github.com/antlr/grammars-v4/blob/master/javascript/JavaScriptParser.g4
            var program = gen.CreateNewProduction("program", false);
            var sourceElement = gen.CreateNewProduction("sourceElement", false);
            var statement = gen.CreateNewProduction("statement", false);
            var block = gen.CreateNewProduction("block", false);
            var statementList = gen.CreateNewProduction("statementList", false);
            var statementListE = gen.CreateNewProduction("statementListE", false);
            var variableStatement = gen.CreateNewProduction("variableStatement", false);
            var variableDeclarationList = gen.CreateNewProduction("variableDeclarationList", false);
            var variableDeclarationListArgs = gen.CreateNewProduction("variableDeclarationListArgs", false);
            var variableDeclaration = gen.CreateNewProduction("variableDeclaration", false);
            var variableDeclarationT = gen.CreateNewProduction("variableDeclarationT", false);
            var emptyStatement = gen.CreateNewProduction("emptyStatement", false);
            var expressionStatement = gen.CreateNewProduction("expressionStatement", false);
            var ifStatement = gen.CreateNewProduction("ifStatement", false);
            var expressionSequenceE = gen.CreateNewProduction("expressionSequenceE", false);
            var iterationStatement = gen.CreateNewProduction("iterationStatement", false);
            var varModifier = gen.CreateNewProduction("varModifier", false);
            var continueStatement = gen.CreateNewProduction("continueStatement", false);
            var breakStatement = gen.CreateNewProduction("breakStatement", false);
            var returnStatement = gen.CreateNewProduction("returnStatement", false);
            var withStatement = gen.CreateNewProduction("withStatement", false);
            var switchStatement = gen.CreateNewProduction("switchStatement", false);
            var caseBlock = gen.CreateNewProduction("caseBlock", false);
            var caseClauses = gen.CreateNewProduction("caseClauses", false);
            var caseClausesE = gen.CreateNewProduction("caseClausesE", false);
            var caseClause = gen.CreateNewProduction("caseClause", false);
            var defaultClause = gen.CreateNewProduction("defaultClause", false);
            var labelledStatement = gen.CreateNewProduction("labelledStatement", false);
            var throwStatement = gen.CreateNewProduction("throwStatement", false);
            var tryStatement = gen.CreateNewProduction("tryStatement", false);
            var catchProduction = gen.CreateNewProduction("catchProduction", false);
            var finallyProduction = gen.CreateNewProduction("finallyProduction", false);
            var debuggerStatement = gen.CreateNewProduction("debuggerStatement", false);
            var functionDeclaration = gen.CreateNewProduction("functionDeclaration", false);
            var classDeclaration = gen.CreateNewProduction("classDeclaration", false);
            var classTail = gen.CreateNewProduction("classTail", false);
            var classElement = gen.CreateNewProduction("classElement", false);
            var classElements = gen.CreateNewProduction("classElements", false);
            var methodDefinition = gen.CreateNewProduction("methodDefinition", false);
            var generatorMethod = gen.CreateNewProduction("generatorMethod", false);
            var formalParameterList = gen.CreateNewProduction("formalParameterList", false);
            var formalParameterArg = gen.CreateNewProduction("formalParameterArg", false);
            var formalParameterArgS = gen.CreateNewProduction("formalParameterArgS", false);
            var lastFormalParameterArg = gen.CreateNewProduction("lastFormalParameterArg", false);
            var functionBody = gen.CreateNewProduction("functionBody", false);
            var sourceElements = gen.CreateNewProduction("sourceElements", false);
            var arrayLiteral = gen.CreateNewProduction("arrayLiteral", false);
            var elementList = gen.CreateNewProduction("elementList", false);
            var lastElement = gen.CreateNewProduction("lastElement", false);
            var objectLiteral = gen.CreateNewProduction("objectLiteral", false);
            var propertyAssignment = gen.CreateNewProduction("propertyAssignment", false);
            var propertyAssignmentS = gen.CreateNewProduction("propertyAssignmentS", false);
            var propertyName = gen.CreateNewProduction("propertyName", false);
            var arguments = gen.CreateNewProduction("arguments", false);
            var lastArgument = gen.CreateNewProduction("lastArgument", false);
            var expressionSequence = gen.CreateNewProduction("expressionSequence", false);
            var singleExpression = gen.CreateNewProduction("singleExpression", false);
            var singleExpressionS = gen.CreateNewProduction("singleExpressionS", false);
            var arrowFunctionParameters = gen.CreateNewProduction("arrowFunctionParameters", false);
            var arrowFunctionBody = gen.CreateNewProduction("arrowFunctionBody", false);
            var assignmentOperator = gen.CreateNewProduction("assignmentOperator", false);
            var literal = gen.CreateNewProduction("literal", false);
            var numericLiteral = gen.CreateNewProduction("numericLiteral", false);
            var identifierName = gen.CreateNewProduction("identifierName", false);
            var reservedWord = gen.CreateNewProduction("reservedWord", false);
            var keyword = gen.CreateNewProduction("keyword", false);
            var getter = gen.CreateNewProduction("getter", false);
            var setter = gen.CreateNewProduction("setter", false);
            var eos = gen.CreateNewProduction("eos", false);
            var star = gen.CreateNewProduction("star", false);

            program |= sourceElements;
            program |= ParserGenerator.EmptyString;

            sourceElements |= sourceElements + sourceElement;
            sourceElements |= ParserGenerator.EmptyString;

            sourceElement |= "Export" + statement;
            sourceElement |= statement;

            statement |= block;
            statement |= variableStatement;
            statement |= emptyStatement;
            statement |= classDeclaration;
            statement |= expressionStatement;
            statement |= ifStatement;
            statement |= iterationStatement;
            statement |= continueStatement;
            statement |= breakStatement;
            statement |= returnStatement;
            statement |= withStatement;
            statement |= labelledStatement;
            statement |= switchStatement;
            statement |= throwStatement;
            statement |= tryStatement;
            statement |= debuggerStatement;
            statement |= functionDeclaration;

            block |= "{" + statementList + "}";

            statementList |= statementListE + statement;
            statementListE |= statementListE + statement;
            statementListE |= ParserGenerator.EmptyString;

            variableStatement |= varModifier + variableDeclarationList + eos;

            variableDeclarationList |= variableDeclaration + variableDeclarationListArgs;
            variableDeclarationListArgs |= "," + variableDeclarationListArgs;
            variableDeclarationListArgs |= ParserGenerator.EmptyString;

            variableDeclaration |= "Identifier" + variableDeclarationT;
            variableDeclaration |= arrayLiteral + variableDeclarationT;
            variableDeclaration |= objectLiteral + variableDeclarationT;

            variableDeclarationT |= "=" + singleExpression;
            variableDeclarationT |= ParserGenerator.EmptyString;

            emptyStatement |= "SemiColon";

            expressionStatement |= expressionSequence + eos; // TODO: 90: {this.notOpenBraceAndNotFunction()}? expressionSequence eos

            ifStatement |= gen.TryCreateNewProduction("If") + "(" + expressionSequence + ")" + statement;
            ifStatement |= gen.TryCreateNewProduction("If") + "(" + expressionSequence + ")" + statement + "Else" + statement;

            expressionSequenceE |= expressionSequence;
            expressionSequenceE |= ParserGenerator.EmptyString;

            iterationStatement |= "Do" + statement + "While" + "(" + expressionSequence + ")" + eos;
            iterationStatement |= gen.TryCreateNewProduction("While") + "(" + expressionSequence + ")" + statement;
            iterationStatement |= gen.TryCreateNewProduction("For") + "(" + expressionSequenceE + ";" + expressionSequenceE + ";" + expressionSequenceE + ")" + statement;
            iterationStatement |= gen.TryCreateNewProduction("For") + "(" + varModifier + variableDeclarationList + ";" + expressionSequenceE + ";" + expressionSequenceE + ")" + statement;
            iterationStatement |= gen.TryCreateNewProduction("For") + "(" + singleExpression + "In" + expressionSequence + ")" + statement;
            iterationStatement |= gen.TryCreateNewProduction("For") + "(" + singleExpression + "Identifier" + expressionSequence + ")" + statement;
            iterationStatement |= gen.TryCreateNewProduction("For") + "(" + varModifier + variableDeclarationList + "In" + expressionSequence + ")" + statement;
            iterationStatement |= gen.TryCreateNewProduction("For") + "(" + varModifier + variableDeclarationList + "Identifier" + expressionSequence + ")" + statement;

            varModifier |= "Var";
            varModifier |= "Let";
            varModifier |= "Const";

            continueStatement |= "Continue" +  eos; // TODO
            continueStatement |= "Continue" + identifierName + eos;

            breakStatement |= "Break" + eos; // TODO
            breakStatement |= "Break" + identifierName + eos;

            returnStatement |= "Return" + eos; // TODO
            returnStatement |= "Return" + expressionSequence + eos;

            withStatement |= gen.TryCreateNewProduction("With") + "(" + expressionSequence + ")" + statement;

            switchStatement |= gen.TryCreateNewProduction("Switch") + "(" + expressionSequence + ")" + caseBlock;

            caseBlock |= gen.TryCreateNewProduction("{") + "}";
            caseBlock |= "{" + caseClauses + "}";
            caseBlock |= "{" + caseClauses + defaultClause + caseClauses + "}";
            caseBlock |= "{" + defaultClause + caseClauses + "}";
            caseBlock |= "{" + caseClauses + defaultClause + "}";
            caseBlock |= "{" + defaultClause + "}";
            
            caseClauses |= caseClausesE + caseClause;
            caseClausesE |= caseClausesE + caseClause;
            caseClausesE |= ParserGenerator.EmptyString;

            caseClause |= "Case" + expressionSequence + ":";
            caseClause |= "Case" + expressionSequence + ":" + statementList;

            defaultClause |= gen.TryCreateNewProduction("Default") + ":";
            defaultClause |= gen.TryCreateNewProduction("Default") + ":" + statementList;

            labelledStatement |= gen.TryCreateNewProduction("Identifier") + ":" + statement;

            throwStatement |= "Throw" + expressionSequence + eos; // TODO

            tryStatement |= gen.TryCreateNewProduction("Try") + block + catchProduction;
            tryStatement |= gen.TryCreateNewProduction("Try") + block + catchProduction + finallyProduction;
            tryStatement |= gen.TryCreateNewProduction("Try") + block + finallyProduction;

            catchProduction |= gen.TryCreateNewProduction("Catch") + "(" + gen.TryCreateNewProduction("Identifier") + ")" + block;

            finallyProduction |= "Finally" + block;

            debuggerStatement |= "Debugger" + eos;

            functionDeclaration |= "Function" + gen.TryCreateNewProduction("Identifier") + "(" + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            functionDeclaration |= "Function" + gen.TryCreateNewProduction("Identifier") + "(" + formalParameterList + ")" + gen.TryCreateNewProduction("{") + functionBody + "}";

            classDeclaration |= gen.TryCreateNewProduction("Class") + "Identifier" + classTail;

            classTail |= "{" + classElements + "}";
            classTail |= "Extends" + singleExpression + "{" + classElements + "}";
            classElements |= classElements + classElement;

            classElement |= methodDefinition;
            classElement |= "Static" + methodDefinition;
            classElement |= "Identifier" + methodDefinition;
            classElement |= "Static" + "Identifier" + methodDefinition;
            classElement |= emptyStatement;

            methodDefinition |= propertyName + "(" + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            methodDefinition |= propertyName + "(" + formalParameterList + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            methodDefinition |= getter + "(" + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            methodDefinition |= setter + "(" + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            methodDefinition |= setter + "(" + formalParameterList + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            methodDefinition |= generatorMethod;

            generatorMethod |= "Identifier" + gen.TryCreateNewProduction("(") + ")" + gen.TryCreateNewProduction("{") + functionBody + "}";
            generatorMethod |= "*" + gen.TryCreateNewProduction("Identifier") + "(" + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            generatorMethod |= gen.TryCreateNewProduction("Identifier") + "(" + formalParameterList + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            generatorMethod |= gen.TryCreateNewProduction("*") + "Identifier" + "(" + formalParameterList + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";

            formalParameterList |= formalParameterArg + formalParameterArgS;
            formalParameterList |= formalParameterArg + "," + lastFormalParameterArg;
            formalParameterList |= formalParameterArg + formalParameterArgS + "," + lastFormalParameterArg;
            formalParameterList |= lastFormalParameterArg;
            formalParameterList |= arrayLiteral;
            formalParameterList |= objectLiteral;

            formalParameterArgS |= "," + formalParameterArg + formalParameterArgS;
            formalParameterArgS |= ParserGenerator.EmptyString;

            formalParameterArg |= "Identifier";
            formalParameterArg |= gen.TryCreateNewProduction("Identifier") + "=" + singleExpression;

            lastFormalParameterArg |= gen.TryCreateNewProduction("Ellipsis") + "Identifier";

            functionBody |= sourceElements;
            functionBody |= ParserGenerator.EmptyString;

            arrayLiteral |= "[" + star + "]";
            arrayLiteral |= "[" + star + elementList + star + "]";

            star |= "*" + star;
            star |= ParserGenerator.EmptyString;

            elementList |= singleExpression + singleExpressionS;
            elementList |= singleExpression + singleExpressionS + "," + lastElement;

            singleExpressionS |= "," + singleExpression + singleExpressionS;
            singleExpressionS |= ParserGenerator.EmptyString;

            lastElement |= gen.TryCreateNewProduction("Ellipsis") + "Identifier";

            objectLiteral |= gen.TryCreateNewProduction("{") + "}";
            objectLiteral |= "{" + gen.TryCreateNewProduction(",") + "}";
            objectLiteral |= "{" + propertyAssignment + propertyAssignmentS + "}";
            objectLiteral |= "{" + propertyAssignment + propertyAssignmentS + gen.TryCreateNewProduction(",") + "}";
            
            propertyAssignmentS |= "," + propertyAssignment + propertyAssignmentS;
            propertyAssignmentS |= ParserGenerator.EmptyString;

            propertyAssignment |= propertyName + ":" + singleExpression;
            propertyAssignment |= propertyName + "=" + singleExpression;
            propertyAssignment |= "[" + singleExpression + "]" + gen.TryCreateNewProduction(":") + singleExpression;
            propertyAssignment |= getter + "(" + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            propertyAssignment |= setter + "(" + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            propertyAssignment |= setter + "(" + formalParameterList + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            propertyAssignment |= generatorMethod;
            propertyAssignment |= identifierName;

            propertyName |= identifierName;
            propertyName |= "StringLiteral";
            propertyName |= numericLiteral;

            arguments |= gen.TryCreateNewProduction("(") + ")";

            arguments |= "(" + singleExpression + singleExpressionS + ")";
            arguments |= "(" + singleExpression + singleExpressionS + "," + lastArgument + ")";

            lastArgument |= gen.TryCreateNewProduction("Ellipsis") + "Identifier";

            expressionSequence |= singleExpression + singleExpressionS;

            singleExpression |= "Function" + gen.TryCreateNewProduction("(") + ")" + gen.TryCreateNewProduction("{") + functionBody + "}";
            singleExpression |= "Function" + gen.TryCreateNewProduction("Identifier") + "(" + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            singleExpression |= "Function" + gen.TryCreateNewProduction("(") + formalParameterList + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            singleExpression |= "Function" + gen.TryCreateNewProduction("Identifier") + "(" + formalParameterList + gen.TryCreateNewProduction(")") + "{" + functionBody + "}";
            singleExpression |= "Class" + classTail;
            singleExpression |= gen.TryCreateNewProduction("Class") + "Identifier" + classTail;
            singleExpression |= singleExpression + "[" + expressionSequence + "]";
            singleExpression |= singleExpression + "." + identifierName;
            singleExpression |= singleExpression + arguments;
            singleExpression |= "New" + singleExpression;
            singleExpression |= "New" + singleExpression + arguments;
            singleExpression |= singleExpression + "++"; // TODO
            singleExpression |= singleExpression + "--"; // TODO
            singleExpression |= "Delete" + singleExpression;
            singleExpression |= "Void" + singleExpression;
            singleExpression |= "Typeof" + singleExpression;
            singleExpression |= "++" + singleExpression;
            singleExpression |= "--" + singleExpression;
            singleExpression |= "+" + singleExpression;
            singleExpression |= "-" + singleExpression;
            singleExpression |= "~" + singleExpression;
            singleExpression |= "!" + singleExpression;
            singleExpression |= singleExpression + "*" + singleExpression;
            singleExpression |= singleExpression + "/" + singleExpression;
            singleExpression |= singleExpression + "%" + singleExpression;
            singleExpression |= singleExpression + "+" + singleExpression;
            singleExpression |= singleExpression + "-" + singleExpression;
            singleExpression |= singleExpression + "<<" + singleExpression;
            singleExpression |= singleExpression + ">>" + singleExpression;
            singleExpression |= singleExpression + ">>>" + singleExpression;
            singleExpression |= singleExpression + "<" + singleExpression;
            singleExpression |= singleExpression + ">" + singleExpression;
            singleExpression |= singleExpression + "<=" + singleExpression;
            singleExpression |= singleExpression + ">=" + singleExpression;

            singleExpression |= singleExpression + "Instanceof" + singleExpression;
            singleExpression |= singleExpression + "In" + singleExpression;
            singleExpression |= singleExpression + "==" + singleExpression;
            singleExpression |= singleExpression + "!=" + singleExpression;
            singleExpression |= singleExpression + "===" + singleExpression;
            singleExpression |= singleExpression + "!==" + singleExpression;
            singleExpression |= singleExpression + "&" + singleExpression;
            singleExpression |= singleExpression + "^" + singleExpression;
            singleExpression |= singleExpression + "|" + singleExpression;
            singleExpression |= singleExpression + "&&" + singleExpression;
            singleExpression |= singleExpression + "||" + singleExpression;
            singleExpression |= singleExpression + "?" + singleExpression + ":" + singleExpression;
            singleExpression |= singleExpression + "=" + singleExpression;
            singleExpression |= singleExpression + assignmentOperator + singleExpression;
            singleExpression |= singleExpression + "TemplateStringLiteral";
            singleExpression |= "This";
            singleExpression |= "Identifier";
            singleExpression |= "Super";
            singleExpression |= literal;
            singleExpression |= arrayLiteral;
            singleExpression |= objectLiteral;
            singleExpression |= "(" + expressionSequence + ")";
            singleExpression |= arrowFunctionParameters + "=>" + arrowFunctionBody;

            arrowFunctionParameters |= "Identifier";
            arrowFunctionParameters |= gen.TryCreateNewProduction("(") + ")";
            arrowFunctionParameters |= "(" + formalParameterList + ")";

            arrowFunctionBody |= singleExpression;
            arrowFunctionBody |= "{" + functionBody + "}";

            assignmentOperator |= "*=";
            assignmentOperator |= "/=";
            assignmentOperator |= "%=";
            assignmentOperator |= "+=";
            assignmentOperator |= "-=";
            assignmentOperator |= "<<=";
            assignmentOperator |= ">>=";
            assignmentOperator |= ">>>=";
            assignmentOperator |= "&=";
            assignmentOperator |= "^=";
            assignmentOperator |= "|=";

            literal |= "NullLiteral";
            literal |= "BooleanLiteral";
            literal |= "StringLiteral";
            literal |= "TemplateStringLiteral";
            literal |= "RegularExpressionLiteral";
            literal |= "numericLiteral";
            
            numericLiteral |= "DecimalLiteral";
            numericLiteral |= "HexIntegerLiteral";
            numericLiteral |= "OctalIntegerLiteral";
            numericLiteral |= "OctalIntegerLiteral2";
            numericLiteral |= "BinaryIntegerLiteral";

            identifierName |= "Identifier";
            identifierName |= reservedWord;

            reservedWord |= keyword;
            reservedWord |= "NullLiteral";
            reservedWord |= "BooleanLiteral";

            keyword |= "Class";
            keyword |= "Break";
            keyword |= "Do";
            keyword |= "Instanceof";
            keyword |= "Typeof";
            keyword |= "Case";
            keyword |= "Else";
            keyword |= "New";
            keyword |= "Var";
            keyword |= "Catch";
            keyword |= "Finally";
            keyword |= "Return";
            keyword |= "Void";
            keyword |= "Continue";
            keyword |= "For";
            keyword |= "Switch";
            keyword |= "While";
            keyword |= "Debugger";
            keyword |= "Function";
            keyword |= "This";
            keyword |= "With";
            keyword |= "Default";
            keyword |= "If";
            keyword |= "Throw";
            keyword |= "Delete";
            keyword |= "In";
            keyword |= "Try";
            keyword |= "Enum";
            keyword |= "Extends";
            keyword |= "Super";
            keyword |= "Const";
            keyword |= "Export";
            keyword |= "Import";
            keyword |= "Implements";
            keyword |= "Let";
            keyword |= "Private";
            keyword |= "Public";
            keyword |= "Interface";
            keyword |= "Package";
            keyword |= "Protected";
            keyword |= "Static";
            keyword |= "Yield";

            //getter |= propertyName; // TODO

            //setter |= propertyName; // TODO

            eos |= "SemiColon"; // TODO
            //eos |= "$";
            //eos |= ParserGenerator.EmptyString;


            gen.PushConflictSolver(true, gen.TryCreateNewProduction("Break"),
                                         gen.TryCreateNewProduction("Do"),
                                         gen.TryCreateNewProduction("Typeof"),
                                         gen.TryCreateNewProduction("Case"),
                                         gen.TryCreateNewProduction("Else"),
                                         gen.TryCreateNewProduction("New"),
                                         gen.TryCreateNewProduction("Var"),
                                         gen.TryCreateNewProduction("Catch"),
                                         gen.TryCreateNewProduction("Finally"),
                                         gen.TryCreateNewProduction("Return"),
                                         gen.TryCreateNewProduction("Void"),
                                         gen.TryCreateNewProduction("Continue"),
                                         gen.TryCreateNewProduction("For"),
                                         gen.TryCreateNewProduction("Switch"),
                                         gen.TryCreateNewProduction("While"),
                                         gen.TryCreateNewProduction("Debugger"),
                                         gen.TryCreateNewProduction("Function"),
                                         gen.TryCreateNewProduction("This"),
                                         gen.TryCreateNewProduction("With"),
                                         gen.TryCreateNewProduction("Default"),
                                         gen.TryCreateNewProduction("If"),
                                         gen.TryCreateNewProduction("Throw"),
                                         gen.TryCreateNewProduction("Delete"),
                                         gen.TryCreateNewProduction("Try"),
                                         gen.TryCreateNewProduction("Enum"),
                                         gen.TryCreateNewProduction("Class"),
                                         gen.TryCreateNewProduction("Extends"),
                                         gen.TryCreateNewProduction("Super"),
                                         gen.TryCreateNewProduction("Const"),
                                         gen.TryCreateNewProduction("Export"),
                                         gen.TryCreateNewProduction("Import"),
                                         gen.TryCreateNewProduction("Implements"),
                                         gen.TryCreateNewProduction("Let"),
                                         gen.TryCreateNewProduction("Private"),
                                         gen.TryCreateNewProduction("Public"),
                                         gen.TryCreateNewProduction("Interface"),
                                         gen.TryCreateNewProduction("Package"),
                                         gen.TryCreateNewProduction("Protected"),
                                         gen.TryCreateNewProduction("Static"),
                                         gen.TryCreateNewProduction("Yield"));

            gen.PushConflictSolver(true, gen.TryCreateNewProduction("Identifier"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("NullLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("BooleanLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("StringLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("TemplateStringLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("RegularExpressionLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("numericLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("DecimalLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("HexIntegerLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("OctalIntegerLiteral"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("OctalIntegerLiteral2"));
            gen.PushConflictSolver(true, gen.TryCreateNewProduction("BinaryIntegerLiteral"));
            
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(ifStatement, 0));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(keyword, 0));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(block, 0));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(statementListE, 1));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(caseClausesE, 1));
            //gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(eos, 1));
            //gen.PushConflictSolver(false, gen.TryCreateNewProduction("Class"));

            gen.PushConflictSolver(false, gen.TryCreateNewProduction("["));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("("));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("{"));

            gen.PushConflictSolver(false, gen.TryCreateNewProduction("."));

            gen.PushConflictSolver(false, gen.TryCreateNewProduction("++"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("--"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("~"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("!"));

            // +, -
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(singleExpression, 16), new Tuple<ParserProduction, int>(singleExpression, 17));

            gen.PushConflictSolver(false, gen.TryCreateNewProduction("*"), gen.TryCreateNewProduction("/"), gen.TryCreateNewProduction("%"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("+"), gen.TryCreateNewProduction("-"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("<<"), gen.TryCreateNewProduction(">>"), gen.TryCreateNewProduction(">>>"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("<"), gen.TryCreateNewProduction(">"), gen.TryCreateNewProduction("<="), gen.TryCreateNewProduction(">="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("Instanceof"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("In"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("=="), gen.TryCreateNewProduction("!="), gen.TryCreateNewProduction("==="), gen.TryCreateNewProduction("!=="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("&"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("^"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("|"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("&&"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("||"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("?"), gen.TryCreateNewProduction(":"));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("="));

            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(singleExpression, 47));

            gen.PushConflictSolver(false, gen.TryCreateNewProduction("*="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("/="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("%="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("+="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("-="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("<<="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction(">>="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction(">>>="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("&="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("^="));
            gen.PushConflictSolver(false, gen.TryCreateNewProduction("|="));

            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(formalParameterArgS, 0));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(formalParameterArgS, 1));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(arrowFunctionBody, 0));

            gen.PushConflictSolver(false, gen.TryCreateNewProduction(","));

            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(singleExpressionS, 1));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(propertyAssignmentS, 1));

            gen.PushConflictSolver(false, gen.TryCreateNewProduction("SemiColon"));

            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(sourceElements, 1));
            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(caseBlock, 0));

            gen.PushConflictSolver(false, gen.TryCreateNewProduction("}"));

            gen.PushConflictSolver(true, new Tuple<ParserProduction, int>(functionBody, 1));
            

            try
            {
                gen.PushStarts(program);
                gen.PrintProductionRules();

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                gen.GenerateLALR();
                var end = sw.ElapsedMilliseconds;
                sw.Stop();
                Console.Console.Instance.WriteLine($"{end.ToString("#,#")}");
                gen.PrintStates();
                gen.PrintTable();
            }
            catch (Exception e)
            {
                Console.Console.Instance.WriteLine(e.Message + "\r\n" + e.StackTrace);
            }

            File.WriteAllText(DateTime.Now.Ticks + ".txt", gen.GlobalPrinter.ToString());

            parser = gen.CreateExtendedShiftReduceParserInstance();
            File.WriteAllText("jsparser.cs", parser.ToCSCode("JSParser"));
        }
    }
}