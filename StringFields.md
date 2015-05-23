A string is a sequence of characters that either has a fixed length
(static or calculated as a result of an expression) or is terminated
with a null byte. Structorian supports both ASCII and Unicode strings.
String values can also be used in expressions. Details on using strings
in expressions are described below, in the section on expression syntax.

The following string types are supported:

| str | ASCII string |
|:----|:-------------|
| cstr | Null-terminated ASCII string |
| wstr | Unicode string |
| char  | ASCII character (alias for **str [len=1]**) |
| wchar | Unicode character (alias for **wstr [len=1]**) |

The following attributes are common to all string types:

**len** = _expression_

> Specifies the length of the string in characters. If this attribute
> is not specified, the string continues until the following null
> character or until the end of file.

> Actually, both **str** and **cstr** fields will read as many bytes as
> specified by the `[len]` attribute, and will stop at the first null
> character encountered. The only difference between them is their
> behavior when editing. For a **str** field of length N, it is allowed
> to enter exactly N characters. On the other hand, for a **cstr** field
> of length N, only N-1 characters can be entered, and the Nth character
> will always be the terminating null byte. **wstr** fields behave like **str** fields,
> and do not enforce the terminating null character.

**value** = _expression_

> Specifies the expression that is evaluated to obtain the value
> of the field. If the `value` attribute is specified, the field
> does not consume any bytes from the data file, and the `len`
> attribute must not be specified.