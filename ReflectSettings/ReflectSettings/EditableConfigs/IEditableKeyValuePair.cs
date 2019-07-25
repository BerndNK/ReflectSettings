namespace ReflectSettings.EditableConfigs
{
    public interface IEditableKeyValuePair : IEditableConfig
    {
        object Key { get; set; }

        object PairValue { get; set; }
    }
}