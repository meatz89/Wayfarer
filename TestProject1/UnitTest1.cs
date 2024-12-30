namespace TestProject1
{
    public class UnitTest1 : IClassFixture<BlazorRPGFixture>
    {
        BlazorRPGFixture Fixture;

        public UnitTest1(BlazorRPGFixture fixture)
        {
            this.Fixture = fixture;
        }

        [Fact]
        public void Test1()
        {
            List<UserActionOption> validActions = Fixture.GameState.ValidUserActions;
            string s = "";
        }
    }
}