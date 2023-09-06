using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AssetKits.ParticleImage;

public class GameController : MonoBehaviour 
{
    //멀티로실제 카지노 블랙잭 테이블처럼 딜러있는거 그대로 쓰고
    //최대 4명까지 방 만들어서
    //칩 제일 많이 딴놈이 1등
    //포톤 서버 연결하고 db쓰고
    //채팅 넣고

    //스프라이트 핸드폰에 있는 롤포커카드 이미지로 바꿔도 될듯
    //가시성이 좀 떨어질거같긴한데

    //-1일때 뒷면이라
    int dealersFirstCard = -1;
    int currentChip = 100;

    public CardStack player;
    public CardStack dealer;
    public CardStack deck;

    public CardStackView playerView;
    public CardStackView dealerView;
    public CardStackView deckView;

    public Button hitButton;
    public Button stickButton;

    //무식하게 껏다켯다하는게 맞는지 모르겠네
    //하위 오브젝트가 많은것도 아니고, 생성 파괴를 하는것도 아니긴한데 몬가
    //파티클도 그냥 껏다켰다하는게 편해서 이래놓기는 했는데
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
        //최고기록 들고오기
        highScore = PlayerPrefs.GetInt("HighScore");
        highScoreText.text = highScore.ToString();
    }

    private void Awake()
    {
        //풀스크린
         Screen.SetResolution(1920, 1080, true);
    }

    #region 버튼온클릭
    public void HitPlay()
    {
        StartCoroutine(Hit());
    }

    IEnumerator Hit()
    {
        //실행되는동안 버튼 비활성화
        hitButton.interactable = false;
        stickButton.interactable = false;
        //덱에서 뽑은 카드를 플레이어한테 추가
        player.Draw(deck.Top());
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
        if (bettingChip > currentChip || bettingChip <= 0)
        {
            logText.text = "올바른 배팅이 아닙니다.";
            return;
        }
        //배팅 UI 끔
        betGroup.SetActive(false);
        //배팅칩 까줌
        currentChip -= bettingChip;
        currentChipText.text = currentChip.ToString();

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
        currentChip = 100;
        PlayAgain();
    }

    //게임종료버튼
    public void EndApp()
    {
        Application.Quit();
    }

   //다음 배팅 버튼
   public void PlayAgain()
    {
        bettingChipText.text = "0";
        mulChipText.text = "";
        currentChipText.text = currentChip.ToString();
        playerView.Clear();
        dealerView.Clear();
        deckView.Clear();
        deck.CreateDeck();
        ResultOff();
        betGroup.SetActive(true);
        dealersFirstCard = -1;
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
        int card = deck.Top();

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
        if (dealer.HandValue() > player.HandValue())
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
            player.Draw(deck.Top());
            HitDealer();
        }

        //첫패가 21이면 블랙잭이니 겜 시작할때 검출
        BackJack();
        logText.text = "";
    }


}
