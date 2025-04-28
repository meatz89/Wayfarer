public static class GameSetup
{
    public static GameState CreateNewGame(string contentDirectory)
    {
        ContentLoader contentLoader = new ContentLoader(contentDirectory);
        return contentLoader.LoadGame();
    }
}