using System.ComponentModel;

namespace SysBot.Pokemon
{
    /// <summary>
    /// Console agnostic settings
    /// </summary>
    public abstract class BaseConfig
    {
        protected const string FeatureToggle = nameof(FeatureToggle);
        protected const string Operation = nameof(Operation);
        private const string Debug = nameof(Debug);

        [Category(FeatureToggle), Description("当启用时，机器人会在不处理任何东西时偶尔按下B按钮(以避免睡眠)。")]
        public bool AntiIdle { get; set; }

        [Category(Debug), Description("在程序启动时跳过创建机器人；有助于测试集成。")]
        public bool SkipConsoleBotCreation { get; set; }

        [Category(Operation)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public LegalitySettings Legality { get; set; } = new();

        [Category(Operation)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public FolderSettings Folder { get; set; } = new();

        public abstract bool Shuffled { get; }
    }
}
