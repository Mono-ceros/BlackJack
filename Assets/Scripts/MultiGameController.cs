using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using AssetKits.ParticleImage;
using UnityEngine.SceneManagement;

public class MultiGameController : MonoBehaviourPunCallbacks
{
    //첫카드 검출하고 값 저장할 변수
    //숫자 저장해야해서 불 말고 인트
    int dealersFirstCard = -1;

    int currentChip = 100;
    int highScore = 0;

    bool isclick = true;

    //방에 따른 칩 배율
    //int roomclass = 1;

    public CardStack player;
    public CardStack dealer;
    public CardStack deck;

    public CardStackView playerView;
    public CardStackView dealerView;
    public CardStackView deckView;

    //하위 오브젝트가 많은것도 아니고, 생성 파괴를 하는것도 아니긴한데
    //무식하게 껏다켯다하는게 괜찮은지 모르겠네
    [Header("UI")]
    public Button hitButton;
    public Button stickButton;
    public GameObject soloEndButton;
    public GameObject playAgainButton;
    public GameObject betGroup;
    public GameObject resultGroup;
    public GameObject playButtonPanel;
    public GameObject gameOverPanel;
    public CanvasGroup playRoleScroll;
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

    PhotonView pv;

    private void OnEnable()
    {
        //최고기록 들고오기
        highScore = PlayerPrefs.GetInt("HighScore");
        if(PlayerPrefs.GetInt("CurrentChip") == 0)
        {
            currentChip = 100;
        }
        else
        {
            currentChip = PlayerPrefs.GetInt("CurrentChip");
        }
        highScoreText.text = highScore.ToString();
        currentChipText.text = currentChip.ToString();
    }

    #region 버튼온클릭

    public void HitPlay()
    {
        StartCoroutine(Hit());
    }

    //내 패가 12인경우라도 : 딜러 공개패 4,5,6일시 스탑
    //내 패가 13~16 : 딜러 공개패 2~6일떄 스탑(딜러 버스트 확률이 높기 때문)
    //내 패가 17~20 : 딜러 공개패가 9~a이고 내 패에 11점의 a가 있을시 히트

    //내 패가 10,11일시 한장만 더 받는다는 조건으로 판돈 두배(더블다운)하는게 유리
    IEnumerator Hit()
    {
        //실행되는동안 버튼 비활성화
        hitButton.interactable = false;
        stickButton.interactable = false;
        //덱에서 뽑은 카드를 플레이어한테 추가
        player.Draw(deck.DeckTop());
        yield return new WaitForSeconds(0.5f);
        //내 패 21넘었는지 체크
        Burst();
        //버튼 활성화
        hitButton.interactable = true;
        stickButton.interactable = true;
    }

    public void Stand()
    {
        StartCoroutine(DealersTurn());
    }

    //배팅버튼으로 게임시작
    public void Bet()
    {
        inputFieldText = inputField.text;
        //입력된 string형식의 숫자를 int로 변환
        //변환 실패시 out에 0
        int.TryParse(inputFieldText, out bettingChip);
        //currentChip이하의 올바른 숫자가 아니면 반환
        //잘되던게 10배수 조건거니까 안되네
        if (bettingChip > currentChip || bettingChip <= 0)
        {
            logText.text = "올바른 배팅이 아닙니다.";
            return;
        }
        soloEndButton.SetActive(false);
        //배팅 UI 끔
        betGroup.SetActive(false);
        //배팅칩 까줌
        currentChip -= bettingChip;
        PlayerPrefs.SetInt("CurrentChip", currentChip);
        currentChipText.text = currentChip.ToString();

        //겜 시작하고 전판 파티클이 남으면 안되니까
        x0Off();
        X2Off();
        X3Off();
        //DealersTurn() 처리를 끝내기 전에 다음판을 시작하면 바로 결과가 나오는 버그가 있음
        //그런줄알았는데 넣어도 버그가 발생하네
        StopAllCoroutines();
        StartGame();
    }

    //파산 후 재시작버튼
    public void NewGame()
    {
        gameOverPanel.SetActive(false);
        //칩 초기화
        currentChip = 100;
        PlayerPrefs.SetInt("CurrentChip", currentChip);
        PlayAgain();
    }

    public void SoloEnd()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void RoleRead()
    {
        playRoleScroll.alpha = (isclick ? 1 : 0);
        playRoleScroll.interactable = isclick;
        playRoleScroll.blocksRaycasts = isclick;

        isclick = isclick ? false : true;
    }

   //다음 배팅 버튼
   public void PlayAgain()
    {
        bettingChipText.text = "0";
        mulChipText.text = "";
        currentChipText.text = currentChip.ToString();
        //게임 초기화
        playerView.Clear();
        dealerView.Clear();
        deckView.Clear();
        deck.CreateDeck();
        dealersFirstCard = -1;
        //UI패널 변경
        ResultOff();
        betGroup.SetActive(true);
        soloEndButton.SetActive(true);
    }

    #endregion

    #region 게임결과
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

    IEnumerator BackJack()
    {
        if (player.HandValue() == 21)
        {
            PlayButtonOff();
            dealerView.Toggle(dealersFirstCard, true);
            dealerView.MakeDeckAndFaceUpUpdate();
            yield return new WaitForSeconds(1f);

            //딜러도 블랙잭이면 무승부
            if (dealer.HandValue() == 21)
            {
                SwitchResult(1);
                ResultOn();
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                SwitchResult(3);
                HighScoreUpdate();
                ResultOn();
                yield return new WaitForSeconds(1.5f);
            }
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
                PlayerPrefs.SetInt("CurrentChip", currentChip);
                break;
            case 2:
                winnerText.text = "승리";
                resultChipText.text = "칩 +" + bettingChip * 2;
                mulChipText.text = "X2";
                currentChip += bettingChip * 2;
                PlayerPrefs.SetInt("CurrentChip", currentChip);
                X2On();
                break;
            case 3:
                winnerText.text = "블랙잭";
                resultChipText.text = "칩 +" + bettingChip * 3;
                mulChipText.text = "X3";
                currentChip += bettingChip * 3;
                PlayerPrefs.SetInt("CurrentChip", currentChip);
                X3On();
                break;
            case 4:
                winnerText.text = "버스트";
                resultChipText.text = "칩 +0";
                mulChipText.text = "X0";
                X0On();
                break;
            case 5:
                winnerText.text = "딜러 블랙잭";
                resultChipText.text = "칩 +0";
                mulChipText.text = "X0";
                X0On();
                break;
            case 6:
                winnerText.text = "파이브 카드 찰리";
                resultChipText.text = "칩 +" + bettingChip * 2;
                mulChipText.text = "X2";
                currentChip += bettingChip * 2;
                PlayerPrefs.SetInt("CurrentChip", currentChip);
                X2On();
                break;
        }
    }

    IEnumerator IsGameOver()
    {
        //정산된 칩이 0이하면 게임오버
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

    #region 온오프
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
        int card = deck.DeckTop();

        //딜러 첫번째 뒷면 카드를
        //게임이 끝날때 앞면으로 만들기 위해서 변수에 저장
        if (dealersFirstCard < 0)
        {
            dealersFirstCard = card;
        }


        dealer.Draw(card);

        ////딜러 두번째 카드부터 앞면
        if (dealer.CardCount >= 2)
        {
            dealerView.Toggle(card, true);
            dealerView.MakeDeckAndFaceUpUpdate();
        }
    }

    IEnumerator DealersTurn()
    {
        PlayButtonOff();

        //딜러의 뒤집어있는 첫카드를 뒤집어줌
        dealerView.Toggle(dealersFirstCard, true);
        dealerView.MakeDeckAndFaceUpUpdate();

        yield return new WaitForSeconds(1f);


        //딜러가 히트하기 전에 졌으면 패배
        if(dealer.HandValue() == 21)
        {
            SwitchResult(5);
            StartCoroutine(IsGameOver());
            yield return new WaitForSeconds(1.5f);
        }
        else if(player.CardCount >= 5)
        {
            SwitchResult(6);
            StartCoroutine(IsGameOver());
            yield return new WaitForSeconds(1.5f);
        }
        else if (dealer.HandValue() > player.HandValue())
        {
            SwitchResult(0);
            StartCoroutine(IsGameOver());
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            //딜러패가 플레이어보다 더 낮을때 딜러 17까지 히트
            while (dealer.HandValue() < 17)
            {
                HitDealer();
                //17이 안됐는데 버스트되지 않게 딜러가 이겨있으면 패배
                if (dealer.HandValue() > player.HandValue() && dealer.HandValue() <= 21)
                {
                    SwitchResult(0);
                    StartCoroutine(IsGameOver());
                    yield return new WaitForSeconds(1.5f);
                    break;
                }
                yield return new WaitForSeconds(1f);
            }

            //결과창이 뜨지 않아있을때(위에서 지지 않았을때) 결과
            if (!resultGroup.activeInHierarchy && !gameOverPanel.activeInHierarchy)
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
        //임시 저장된 최고기록보다 현재 칩이 더 높으면
        //최고기록 갱신
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

        //플래이어 딜러 카드 2장씩
        for (int i = 0; i < 2; i++)
        {
            player.Draw(deck.DeckTop());
            HitDealer();
        }

        //첫패가 21이면 블랙잭이니 겜 시작할때 검출
        StartCoroutine(BackJack());
        logText.text = "";
    }


}
