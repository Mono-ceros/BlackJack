using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
    public GameObject PlayAgainButton;

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
    int highScore = 0;

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore");
        highScoreText.text = highScore.ToString();
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
        hitButton.interactable = false;
        stickButton.interactable = false;
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
            winnerText.text = "버스트";
            resultChipText.text = "칩 +0";
            HighScoreUpdate();

            StartCoroutine(IsGameOver());
        }
    }

    void BackJack()
    {
        if (player.HandValue() == 21)
        {
            PlayButtonOff();
            winnerText.text = "블랙잭";
            float tempnumber = bettingChip * 2.5f;
            resultChipText.text = "칩 +" + (int)Mathf.Round(tempnumber);
            currentChip += (int)Mathf.Round(tempnumber);
            HighScoreUpdate();
            ResultOn();
        }
    }

    void OutCome()
    {
        if (dealer.HandValue() == player.HandValue())
        {
            winnerText.text = "무승부";
            resultChipText.text = "칩 +" + bettingChip;
            currentChip += bettingChip;
        }
        else if (dealer.HandValue() >= player.HandValue() && dealer.HandValue() <= 21)
        {
            winnerText.text = "패배";
            resultChipText.text = "칩 +0";
        }
        else if (dealer.HandValue() > 21 || (player.HandValue() <= 21 && player.HandValue() > dealer.HandValue()))
        {
            winnerText.text = "승리";
            resultChipText.text = "칩 +" + (bettingChip * 2);
            currentChip += bettingChip * 2;
        }
    }

    IEnumerator IsGameOver()
    {
        if (currentChip <= 0)
        {
            winnerText.text = "파산";
            ResultOn();
            PlayAgainButton.SetActive(false);
            yield return new WaitForSeconds(1f);
            ResultOff();
            PlayAgainButton.SetActive(true);
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

        //딜러 패가 17이상이 될때까지 딜러히트
        while (dealer.HandValue() < 17)
        {
            HitDealer();
            yield return new WaitForSeconds(1f);
        }

        OutCome();
        HighScoreUpdate();
        StartCoroutine(IsGameOver());
        yield return new WaitForSeconds(1f);
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
        currentChipText.text = currentChip.ToString();
        player.GetComponent<CardStackView>().Clear();
        dealer.GetComponent<CardStackView>().Clear();
        deck.GetComponent<CardStackView>().Clear();
        deck.CreateDeck();
        ResultOff();
        betGroup.SetActive(true);

        hitButton.interactable = true;
        stickButton.interactable = true;

        dealersFirstCard = -1;
    }

}
