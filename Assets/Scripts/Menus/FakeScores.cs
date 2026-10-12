using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class FakeScores : MonoBehaviour
{
    private List<string> nomes = new List<string>
    {
        "Alice", "Bob", "Charlie", "David", "Eve",
        "Frank", "Grace", "Heidi", "Daniel", "Ethan",
        "Fiona", "George", "Hannah", "Ian", "Jack",
        "Karen", "Leo", "Mia", "Nina", "Oscar"
    };

    public async Task GerarFakeScores()
    {
        for (int i = 0; i < 20; i++)
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }
            AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            string nome = nomes[i];
            await AuthenticationService.Instance.UpdatePlayerNameAsync(nome);

            int score = Random.Range(1000, 100000);
            await LeaderboardsService.Instance.AddPlayerScoreAsync("InfinityScore", score);
        }
    }

}
