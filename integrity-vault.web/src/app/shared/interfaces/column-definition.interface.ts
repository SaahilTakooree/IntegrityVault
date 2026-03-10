// Generic interfaces describing the configuration for column in a reusable table componenet.
export interface IColumnDefinition<T> {
    key: keyof T; // The property name of from generic type T that this column will displayed.
    label: string; // The display text shown in the table header for this column.
    mono: boolean; // Indicates the column content should use a monospace font.
    truncate?: boolean; // Optional flag to truncate long text with ellipsis in the UI instead of wrapping.
    transform?: (value : any) => string; // Optional to transform the a set of values into another one.
}