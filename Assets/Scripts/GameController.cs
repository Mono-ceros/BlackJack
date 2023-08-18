using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AssetKits.ParticleImage;

public class GameController : MonoBehaviour 
{
    int dealersFirstCard = -1;
    int currentChip = 100;

    public CardStack player;
    public CardStack dealer;
    public CardStack deck;

    public Button hitButton;
    public Button stickButton;

    public GameObject betGroup;
    public GameObject resultGroup;
    public GameObject playButtonPanel;
    public GameObject gameOverPanel;
    public GameObject playAgainButton;
    public GameObject x2;
    public GameObject x3;
    public GameObject x0;

    public InputField inputField;
    string inputFieldText;
    int bettingChip;

    public Text winnerText;
    public Text playerHandValueText;
    public Text dealerHandValueText;
    public Text currentChipText;
    public Text bettingChipText;
    public Text logText;
    public Text resultChipText;
    public Text highScoreText;
    public Text mulChipText;
    int highScore = 0;

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore");
        highScoreText.text = highScore.ToString();
    }

    private void Awake()
    {
         Screen.SetResolution(1920, 1080, true);
    }

    #region Button
    public void HitPlay()
    {
        StartCoroutine(Hit());
    }

    IEnumerator Hit()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        player.Push(deck.Pop());
        yield return new WaitForSeconds(0.5f);
        Burst();
        hitButton.interactable = true;
        stickButton.interactable = true;
    }

    public void Stand()
    {
        StartCoroutine(DealersTurn());
    }

    public void Bet()
    {
        inputFieldText = inputField.text;
        int.TryParse(inputFieldText, out bettingChip);
        if(bettingChip > currentChip || bettingChip <= 0)
        {
            logText.text = "올바른 배팅이 아닙니다.";
            return;
        }
        betGroup.SetActive(false);
        currentChip -= bettingChip;
        currentChipText.text = currentChip.ToString();
        x0Off();
        X2Off();
        X3Off();
        StartGame();
    }

    public void NewGame()
    {
        gameOverPanel.SetActive(false);
        currentChip = 100;
        PlayAgain();
    }

    public void EndApp()
    {
        Application.Quit();
    }

    #endregion

    #region Result
    void Burst()
    {
        if (player.HandValue() > 21)
        {
            PlayButtonOff();
            SwitchResult(4);
            HighScoreUpdate();
            StartCoroutine(IsGameOver());
        }
    }

    void BackJack()
    {
        if (player.HandValue() == 21)
        {
            PlayButtonOff();
            SwitchResult(3);
            HighScoreUpdate();
            ResultOn();
        }
    }

    void OutCome()
    {
        if (dealer.HandValue() == player.HandValue())
        {
            SwitchResult(1);
        }
        else if (dealer.HandValue() > player.HandValue() && dealer.HandValue() <= 21)
        {
            SwitchResult(0);
        }
        else if (dealer.HandValue() > 21 || player.HandValue() > dealer.HandValue())
        {
            SwitchResult(2);
        }
    }

    void SwitchResult(int resultint)
    {
        switch (resultint)
        {
            case 0:
                winnerText.text = "패배";
                resultChipText.text = "칩 +0";
                mulChipText.text = "X0";
                X0On();
                break;
            case 1:
                winnerText.text = "무승부";
                resultChipText.text = "칩 +" + bettingChip;
                currentChip += bettingChip;
                break;
            case 2:
                winnerText.text = "승리";
                resultChipText.text = "칩 +" + (bettingChip * 2);
                mulChipText.text = "X2";
                currentChip += bettingChip * 2;
                X2On();
                break;
            case 3:
                winnerText.text = "블랙잭";
                resultChipText.text = "칩 +" + bettingChip * 3;
                mulChipText.text = "X3";
                currentChip += bettingChip * 3;
                X3On();
                break;
            case 4:
                winnerText.text = "버스트";
                resultChipText.text = "칩 +0";
                mulChipText.text = "X0";
                X0On();
                break;
        }
    }

    IEnumerator IsGameOver()
    {
        if (currentChip <= 0)
        {
            winnerText.text = "파산";
            ResultOn();
            playAgainButton.SetActive(false);
            yield return new WaitForSeconds(1f);
            ResultOff();
            playAgainButton.SetActive(true);
            gameOverPanel.SetActive(true);
        }
        else
        {
            ResultOn();
        }
    }

    #endregion

    #region OnOff
    void PlayButtonOn()
    {
        playButtonPanel.SetActive(true);
    }

    void PlayButtonOff()
    {
        playButtonPanel.SetActive(false);
    }

    void ResultOn()
    {
        playerHandValueText.text = player.HandValue().ToString();
        dealerHandValueText.text = dealer.HandValue().ToString();
        resultGroup.SetActive(true);
    }

    void ResultOff()
    {
        resultGroup.SetActive(false);
    }

    void X0On()
    {
        x0.SetActive(true);
    }

    void x0Off()
    {
        x0.SetActive(false);
    }

    void X2On()
    {
        x2.SetActive(true);
    }

    void X2Off()
    {
        x2.SetActive(false);
    }

    void X3On()
    {
        x3.SetActive(true);
    }

    void X3Off()
    {
        x3.SetActive(false);
    }

    #endregion

    #region Dealer
    void HitDealer()
    {
        int card = deck.Pop();

        if (dealersFirstCard < 0)
        {
            dealersFirstCard = card;
        }

        dealer.Push(card);
        if (dealer.CardCount >= 2)
        {
            CardStackView view = dealer.GetComponent<CardStackView>();
            view.Toggle(card, true);
        }
    }

    IEnumerator DealersTurn()
    {
        PlayButtonOff();

        CardStackView view = dealer.GetComponent<CardStackView>();
        view.Toggle(dealersFirstCard, true);
        view.ShowCards();

        yield return new WaitForSeconds(1f);


        if (dealer.HandValue() > player.HandValue())
        {
            SwitchResult(0);
            StartCoroutine(IsGameOver());
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            //딜러가 이긴건 아닌데 17이 넘는 경우
            //딜러 패가 17이상이 될때까지 딜러히트
            while (dealer.HandValue() < 17)
            {
                HitDealer();
                if (dealer.HandValue() > player.HandValue() && dealer.HandValue() <= 21)
                {
                    SwitchResult(0);
                    StartCoroutine(IsGameOver());
                    yield return new WaitForSeconds(1.5f);
                    break;
                }
                yield return new WaitForSeconds(1f);
            }

            if (!resultGroup.activeInHierarchy && !playAgainButton.activeInHierarchy)
            {
                OutCome();
                HighScoreUpdate();
                StartCoroutine(IsGameOver());
                yield return new WaitForSeconds(1.5f);
            }
            
        }
    }

    #endregion

    void HighScoreUpdate()
    {
        if (currentChip > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", currentChip);
            highScore = PlayerPrefs.GetInt("HighScore");
            highScoreText.text = highScore.ToString();
        }
    }

    void StartGame()
    {
        bettingChipText.text = bettingChip.ToString();
        currentChipText.text = currentChip.ToString();
        PlayButtonOn();

        for (int i = 0; i < 2; i++)
        {
            player.Push(deck.Pop());
            HitDealer();
        }

        BackJack();
        logText.text = "";
    }

    public void PlayAgain()
    {
        bettingChipText.text = "0";
        mulChipText.text = "";
        currentChipText.text = currentChip.ToString();
        player.GetComponent<CardStackView>().Clear();
        dealer.GetComponent<CardStackView>().Clear();
        deck.GetComponent<CardStackView>().Clear();
        deck.CreateDeck();
        ResultOff();
        betGroup.SetActive(true);
        dealersFirstCard = -1;
    }

}
