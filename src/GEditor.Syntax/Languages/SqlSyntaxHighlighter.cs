namespace GEditor.Syntax;

public sealed class SqlSyntaxHighlighter : RegexBasedHighlighter
{
    public override string LanguageName => "SQL";
    public override IReadOnlySet<string> SupportedExtensions => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".sql", ".ddl", ".dml" };

    private static readonly IReadOnlyList<HighlightRule> Rules = new HighlightRule[]
    {
        // 注释
        new() { Pattern = Compile(@"--.*$"), Kind = TokenKind.Comment },
        new() { Pattern = Compile(@"/\*[\s\S]*?\*/"), Kind = TokenKind.Comment },
        
        // 字符串
        new() { Pattern = Compile(@"'(?:[^'']|'')*'"), Kind = TokenKind.String },
        
        // 关键字（大写优先）
        new() { Pattern = Compile(@"\b(?:SELECT|FROM|WHERE|AND|OR|NOT|IN|EXISTS|BETWEEN|LIKE|IS|NULL|AS|ON|JOIN|INNER|LEFT|RIGHT|OUTER|FULL|CROSS|UNION|ALL|DISTINCT|ORDER|BY|ASC|DESC|GROUP|HAVING|LIMIT|OFFSET|INSERT|INTO|VALUES|UPDATE|SET|DELETE|CREATE|TABLE|INDEX|VIEW|DROP|ALTER|ADD|COLUMN|CONSTRAINT|PRIMARY|KEY|FOREIGN|REFERENCES|CHECK|DEFAULT|UNIQUE|CASE|WHEN|THEN|ELSE|END|IF|WHILE|BEGIN|COMMIT|ROLLBACK|SAVEPOINT|GRANT|REVOKE|TRIGGER|PROCEDURE|FUNCTION|RETURN|DECLARE|EXEC|EXECUTE|WITH|RECURSIVE|OVER|PARTITION|RANGE|ROWS|UNBOUNDED|PRECEDING|FOLLOWING|CURRENT|ROW|EXCLUDE|GROUPS|TIES|FETCH|FIRST|NEXT|ONLY|FOR|UPDATE|OF|SHARE|NOWAIT|SKIP|LOCKED|NO|CYCLE|SET|SEARCH|DEPTH|BREADTH)\b"),
            Kind = TokenKind.Keyword },
        
        // 数据类型
        new() { Pattern = Compile(@"\b(?:INT|INTEGER|BIGINT|SMALLINT|TINYINT|DECIMAL|NUMERIC|FLOAT|REAL|DOUBLE|PRECISION|CHAR|VARCHAR|TEXT|STRING|BLOB|BINARY|VARBINARY|BOOLEAN|BOOL|DATE|DATETIME|TIMESTAMP|TIME|YEAR|UUID|JSON|XML|ARRAY|ENUM|SET|GEOMETRY|POINT|LINESTRING|POLYGON|BIT|VARBIT|BYTEA|CLOB|NCLOB|INTERVAL|SERIAL|BIGSERIAL|MONEY|INET|CIDR|MACADDR)\b"),
            Kind = TokenKind.Type },
        
        // 数字
        new() { Pattern = Compile(@"\b\d+(?:\.\d+)?(?:[eE][+-]?\d+)?\b"), Kind = TokenKind.Number },
        new() { Pattern = Compile(@"\b0[xX][0-9a-fA-F]+\b"), Kind = TokenKind.Number },
        
        // 系统函数
        new() { Pattern = Compile(@"\b(?:COUNT|SUM|AVG|MIN|MAX|COALESCE|NULLIF|CAST|CONVERT|EXTRACT|DATE_PART|SUBSTR|SUBSTRING|LENGTH|UPPER|LOWER|TRIM|LTRIM|RTRIM|REPLACE|CONCAT|POSITION|OVERLAY|ABS|CEIL|FLOOR|ROUND|RANDOM|RAND|NOW|CURRENT_DATE|CURRENT_TIME|CURRENT_TIMESTAMP|LOCALTIME|LOCALTIMESTAMP|USER|CURRENT_USER|SESSION_USER|DATABASE|SCHEMA|VERSION)\s*\("),
            Kind = TokenKind.Preprocessor },
        
        // 特殊值
        new() { Pattern = Compile(@"\b(?:TRUE|FALSE|UNKNOWN)\b"), Kind = TokenKind.Number },
    };

    protected override IReadOnlyList<HighlightRule> GetRules() => Rules;
}
