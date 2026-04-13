namespace GEditor.Syntax;

/// <summary>
/// Token 语义类型枚举 — 只表达"这是什么"，不表达"什么颜色"。
/// UI 层负责将 TokenKind 映射到具体颜色/样式。
/// </summary>
public enum TokenKind
{
    None,           // 普通文本 / 未分类
    Keyword,        // 语言关键字
    String,         // 字符串字面量
    Comment,        // 注释
    Number,         // 数字字面量
    Identifier,     // 标识符 / 变量名 / 函数名
    Operator,       // 运算符
    Delimiter,      // 分隔符 / 括号
    Type,           // 类型名
    Attribute,      // 特性 / 注解
    Preprocessor,   // 预处理指令
    PlainText       // 纯文本文件，无高亮
}
