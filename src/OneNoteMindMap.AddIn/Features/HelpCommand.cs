using OneNoteMindMap.UI;

namespace OneNoteMindMap.Features
{
    public static class HelpCommand
    {
        public static void Execute()
        {
            HelpForm.ShowHelp(DialogHost.OneNoteWindow);
        }
    }
}
