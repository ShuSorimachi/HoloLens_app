using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine.UI;


using WebSocketSharp;


public class imgAdmin : MonoBehaviour {

    public GameObject holoCamera;  //カメラ回りのオブジェクトの角度調整用
    public GameObject cameraEmpty;

    public GameObject prefab; //Quadの変数    

    static int numOb = 3;
    public GameObject[] pParent = new GameObject[numOb]; //ウィンドウの親オブジェクト
    GameObject[] p = new GameObject[numOb];    //複製したウィンドウ格納用変数
    Renderer[] editRend = new Renderer[numOb]; //↑のRenderer格納用
    Texture2D[] rootTex = new Texture2D[numOb]; //編集前の元のテクスチャ格納用変数
    int[] scrollNum = new int[numOb];
    bool[] activate = new bool[numOb];

  //  Texture2D edit;


    int gbNum = 0; //配列管理用変数

    public string Server = "http://54.95.63.119:8050/"; //サーバのアドレス
    public float WaitTime = 2;   //待ち時間
    public bool RenderAll = true;    //ページ全体をスクショするか


    float num = 0f;   //Instantiateする座標をずらすやつ

    // public GameObject[] testGb = new GameObject[2];
    // Renderer[] testRend = new Renderer[2];

    public GameObject cursorObject;
    public GameObject cursorRotate;

    //WebSocketのやつ
    //private WebSocket ws = new WebSocket("ws://18.179.71.51:8080/");
    private WebSocket ws = new WebSocket("ws://54.95.63.119:8080/");

    PosData posData1 = new PosData();  //JSON用オブジェクト
    PosData posData2 = new PosData();

    bool[] touch = new bool[2];
    private int posNum = 0;

    bool touchOn = false;  //タッチの種類判定用変数
    bool touching = false;
    bool touchUp = false;
    bool dontTouch = true;

    float touchPosX1; //タッチした瞬間の座標
    float touchPosY1;
    float touchPosX2; //タッチした瞬間の座標2
    float touchPosY2;

    float[] nowPosX1 = new float[2]; //現在タッチしている座標
    float[] nowPosY1 = new float[2];
    float[] nowPosX2 = new float[2]; //現在タッチしている座標2
    float[] nowPosY2 = new float[2];

    float a = 0.8f;

    // Vector3 rotateVec = new Vector3(0f, 0f, 0f); //カーソルを動かす用のベクトル
    float rotateX;
    float rotateY;

    int tapNum = 0;
    int tapNumFinal = 0;
    bool tapMove = false;

    float touchTimer = 0f;  //タップ判定用
    float touchTimer2 = 0f;

    float singleTapTimer = 0f;

    public Renderer cursorShader;
    bool spawnWeb = false; //ウィンドウ生成の重複を防ぐやつ
    string webURL = "";

    bool touchFin = false;
    float touchFinTime = 0f;

    //Vuforia関連
    bool[] ar = new bool[2];

    void Start()
    {
        for(int i = 0; i < scrollNum.Length; i++)
        {
            scrollNum[i] = 0;
            activate[i] = false;
        }

        //SpawnWebPage("http://www.yoshinoya.com/menu/");
        //SpawnWebPage("https://amateras.wsd.kutc.kansai-u.ac.jp/");
        num = 3f;
      //  SpawnWebPage("https://www.matsuyafoods.co.jp/menu/");

    //    testRend[0] = testGb[0].GetComponent<Renderer>();
    //    testRend[1] = testGb[1].GetComponent<Renderer>();


        //WebSocketのやつ
        // 接続開始時のイベント.
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Opended");
        };
        // メッセージ受信時のイベント.
        ws.OnMessage += (sender, e) =>
        {

            // Debug.Log("Received " + e.Data);
            
            posData2 = posData1;
            posData1 = JsonUtility.FromJson<PosData>(e.Data);

          //  touch[0] = touch[1];
            if (posData1.posNum > 0)
            {
                touch[1] = true;
            }
            else
            {
                touch[1] = false;
            }

            posNum = posData1.posNum;

           /* if (posData1.posNum == 0)
            {
                touch[1] = false;
            }*/

          //  Debug.Log(touch[0] + ":" + touch[1]);

        };
        // 接続.
#if WINDOWS_UWP
        ws.ConnectAsync();
# else  
        ws.Connect();
#endif
        // メッセージ送信.
        ws.Send("HelloWorld");

    }

    void Update()
    {

        cameraEmpty.transform.position = holoCamera.transform.position;
        touchJudge(); //タッチの判定用関数
        activation();  //ウィンドウ有効化用関数

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnWebPage("http://www.yoshinoya.com/menu/");
        }
    }

    public void OpenPage(GameObject keyword, int Gnum, string url)    //ページ開く関数
    {

        StartCoroutine(GetTexture(keyword, Gnum, url));
    }

    IEnumerator GetTexture(GameObject keyword, int Gnum, string url)   //テスクチャを入手するためのコルーチン
    {

        NameValueCollection parameters = new NameValueCollection
        {

            { "url", url},
            { "wait", WaitTime.ToString() },
            { "width", "1024" },
            { "render_all", RenderAll ? "1" : "0" }
        };

        string uri = Server + "render.jpeg" + ToQueryString(parameters);
        using (UnityWebRequest www = UnityWebRequest.GetTexture(uri))
        {
            yield return www.Send();

            if (www.isError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                var width = myTexture.width;
                var height = myTexture.height;


                var rectTransform = GetComponent<RectTransform>();

                if (rectTransform)
                {
                    var size = rectTransform.sizeDelta;
                    rectTransform.sizeDelta = new Vector2(size.x, size.x * height / width);
                }

      //          var rawImage = GetComponent<RawImage>();

                /*if (rawImage)

                {

                    rawImage.texture = myTexture;

                }*/

              /*  Renderer rend;
                rend = keyword.GetComponent<Renderer>();*/
                rootTex[Gnum] = myTexture;  //元のテクスチャを格納

                CutPage(keyword, rootTex[Gnum], Gnum, 0);
            }
        }
    }



    private string ToQueryString(NameValueCollection nvc)

    {
        return "?" + string.Join("&", nvc.AllKeys.Select(key => string.Format("{0}={1}", WWW.EscapeURL(key), WWW.EscapeURL(nvc[key]))).ToArray());
    }

    Texture2D getWindowTexture(Texture2D root, int numY)   //一部を切り取るための関数
    {
        Texture2D editTex;
        Color[] pixel;
        int width = root.width;
        int height = root.height;

        pixel = root.GetPixels(0, numY, width, 720);
        editTex = new Texture2D(width, 720);

        editTex.SetPixels(pixel);
        editTex.Apply();

        return editTex;

    }

    void CutPage(GameObject gb, Texture2D tex, int num, int scrollPos)    //元の画像から一部を切り抜く関数 (quad,元のテクスチャ,配列管理用変数,スクロール用変数)
    {
        Texture2D tmpTex;

        tmpTex = getWindowTexture(tex, tex.height - 720 - scrollPos);  //元画像から一部分を切り抜く
        Destroy(editRend[num].material.mainTexture);
        editRend[num].material.mainTexture = tmpTex;                 //テクスチャとして設定する
        gb.transform.localScale = new Vector3(3f, 3f * tmpTex.height / tmpTex.width, 1f);   //大きさを変更

    }

    void SpawnWebPage(string urlC)    //ウィンドウを生成するための関数
    {
        for (int i = 0; i < p.Length; i++)
        {
            if (p[i] == null)
            {
                p[i] = Instantiate(prefab, pParent[i].transform.position+new Vector3(0f,0f,7.3f)/* + new Vector3(num, 0f, 0f)*/, transform.rotation);   //ウィンドウの複製
                p[i].transform.parent = pParent[i].transform;
                pParent[i].transform.rotation = holoCamera.transform.rotation;
                editRend[i] = p[i].GetComponent<Renderer>();
                OpenPage(p[i], i, urlC);   //テクスチャの取得
                break;
            }
        }
    }

    void Scroll(int numWindow)   //スクロールさせる関数 (スクロールさせてる座標)
    {
        if (Input.GetKey(KeyCode.T))
        {
            if (rootTex[numWindow].height - 720 - scrollNum[numWindow] < rootTex[numWindow].height - 720)
            {

                scrollNum[numWindow]--;
            }
        }  
        else if (Input.GetKey(KeyCode.G))
        {
            if (rootTex[numWindow].height - 720 - scrollNum[numWindow] > 0)
            {
                scrollNum[numWindow]++;
            }
        }
        
        if(posNum == 2) {  //タッチしている箇所が2箇所以上の場合スクロールさせる
            if (rootTex[numWindow].height - 720 - scrollNum[numWindow] <= rootTex[numWindow].height - 720 && rootTex[numWindow].height - 720 - scrollNum[numWindow] >= 0)  //重たくなっているのはおそらくここか次のif文
            {
            //    Debug.Log(10f * (nowPosX1[1] - touchPosX1) * Time.deltaTime);
                scrollNum[numWindow]+= Mathf.RoundToInt(10f * (nowPosY1[1] - touchPosY1) * Time.deltaTime *-1);

                
            }

            if (rootTex[numWindow].height - 720 - scrollNum[numWindow] > rootTex[numWindow].height - 720)
            {
                scrollNum[numWindow] = 0;
            }

            if (rootTex[numWindow].height - 720 - scrollNum[numWindow] < 0)
            {
                scrollNum[numWindow] = rootTex[numWindow].height -720;
            }

            if (editRend[numWindow] != null && editRend[numWindow].material.mainTexture != null)
            {

                CutPage(p[numWindow], rootTex[numWindow], numWindow, scrollNum[numWindow]);
            }
        }

      /*  if (editRend[numWindow] != null && editRend[numWindow].material.mainTexture != null)
        {

            CutPage(p[numWindow], rootTex[numWindow], numWindow, scrollNum[numWindow]);
        }*/
    }

    void Move(int numWindow)
    {
        if (singleTapTimer > 2f)
        {
            tapMove = true;
            
                //83 96 237
        }

        if(tapMove == true)
        {
            cursorShader.material.color = new Color(255f, 0f, 0f);
            //   p[numWindow].transform.RotateAround(holoCamera.transform.position, rotateVec, 0.1f * rotateVec.magnitude * Time.deltaTime);
            //   p[numWindow].transform.rotation = new Quaternion(p[numWindow].transform.rotation.x, p[numWindow].transform.rotation.y, 0f, p[numWindow].transform.rotation.w);
            pParent[numWindow].transform.rotation = Quaternion.Euler(rotateX, rotateY, 0f);
        }
    }

 /*   int ClickGb()   //アクティブにするウィンドウを切り替えるための関数
    {
        GameObject gb = null;
        int num = -1;
        bool numC = false;

        if (Input.GetMouseButton(0))
        {
            Ray ray = new Ray();
            RaycastHit hit = new RaycastHit();
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))   //Rayを飛ばして当たったObjectを格納
            {
                gb = hit.collider.gameObject;
            }

            for (int i = 0; i < p.Length; i++)
            {

                if (p[i] == gb)   //あたったオブジェクトと同じものを探してその配列番号を格納する
                {
                    num = i;
                    numC = true;
                }
            }

            if(numC == false)
            {
                num = -2;
            }
        }

        return num;    //クリックしたオブジェクトの配列番号を返す
    }*/

    int ClickGb(int tap)   //アクティブにするウィンドウを切り替えるための関数
    {
        GameObject gb = null;
        int num = -1;
        bool numC = false;

        // Ray ray = new Ray(cameraEmpty.transform.position, cursorObject.transform.position);
        Ray ray = new Ray(cursorObject.transform.position, cursorObject.transform.forward);
        RaycastHit hit = new RaycastHit();

        if (tap == 1)  //タップ回数が1なら
        {

            if (Physics.Raycast(ray, out hit))   //Rayを飛ばして当たったObjectを格納  
            {
                
                gb = hit.collider.gameObject;
                if (gb.gameObject.name == "yoshinoya")
                {
                    spawnWeb = true;
                    //SpawnWebPage("http://www.yoshinoya.com/menu/");
                    //webURL = "https://www.yoshinoya.com/menu/";
                    webURL = "http://act.qrtranslator.com/0016000011/000001";
                }
                else if (hit.transform.name == "matsuya")
                {
                    spawnWeb = true;
                    // SpawnWebPage("https://www.matsuyafoods.co.jp/menu/gyumeshi/index.html");
                    //webURL = "https://www.matsuyafoods.co.jp/menu/gyumeshi/index.html";
                    webURL = "https://www.matsuyafoods.co.jp/english/menu/gyumeshi/index.html";
                }
                else if (hit.transform.name == "isomaru")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://r.gnavi.co.jp/rv2dexm70000/menu3/");
                    //webURL = "https://r.gnavi.co.jp/rv2dexm70000/menu3/";
                    webURL = "https://gurunavi.com/en/n069404/mn/recommend/rst/";
                }
                else if (hit.transform.name == "toriki")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://www.torikizoku.co.jp/menu/yakitori");
                    webURL = "https://www.torikizoku.co.jp/menu/yakitori";
                }
                else if (hit.transform.name == "uotami")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://r.gnavi.co.jp/kaed350/menu2/");
                    //webURL = "https://r.gnavi.co.jp/kaed350/menu2/";
                    webURL = "https://gurunavi.com/en/all?q=uotami%20";
                }
                else if (hit.transform.name == "watami")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://watami-zawatami.com/menu/food/");
                    webURL = "https://watami-zawatami.com/menu/food/";
                }
                else if (hit.transform.name == "yayoi")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://www.yayoiken.com/sp/menu_list/index/2");
                    webURL = "https://www.yayoiken.com/sp/menu_list/index/2";
                    //webURL = "https://www.yayoiken.com/en_files/regular_menu_pdf/31_2019.10Regular_EN.pdf?id=1479";
                }
                else if (hit.transform.name == "domadoma")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://r.gnavi.co.jp/k574812/menu4/");
                    webURL = "https://r.gnavi.co.jp/k574812/menu4/";
                    //webURL = "https://www.doma-doma.com/international/pdf/all_international.pdf";
                }
                else if (hit.transform.name == "rairaitei")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://www.rairaitei.co.jp/menu/");
                    webURL = "https://www.rairaitei.co.jp/menu/";
                }
                else if (hit.transform.name == "juukaru")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://www.tomato-a.co.jp/jyujyukarubi/menu/single.html");
                    webURL = "https://www.tomato-a.co.jp/jyujyukarubi/menu/single.html";
                }
                else if (hit.transform.name == "kfc")
                {
                    spawnWeb = true;
                    //SpawnWebPage("https://www.kfc.co.jp/menu/");
                    webURL = "https://www.kfc.co.jp/menu/";
                    //webURL = "https://prd-kfc.s3.amazonaws.com/pdf/menu/menu_en.pdf";
                }
                else if (hit.transform.name == "macdonald")
                {
                    spawnWeb = true;
                    webURL = "http://www.mcdonalds.co.jp/menu/burger/";
                }
                else if (hit.transform.name == "staba")
                {
                    spawnWeb = true;
                    //webURL = "http://www.starbucks.co.jp/beverage/";
                    webURL = "https://www.starbucks.co.jp/en/beverage/espresso.html";
                }
                else if (hit.transform.name == "marukame")
                {
                    spawnWeb = true;
                    //webURL = "https://www.marugame-seimen.com/menu/";
                    webURL = "https://marugameudon.com/new-sawtelle-menu/";
                }
                else if (hit.transform.name == "lotteria")
                {
                    spawnWeb = true;
                    webURL = "https://www.lotteria.jp/menu/category.php?c=burger";
                }
                else if (hit.transform.name == "sukiya")
                {
                    spawnWeb = true;
                    //webURL = "https://www.sukiya.jp/menu/in/gyudon/";
                    webURL = "https://www.sukiya.jp/en/";
                }
                else if (hit.transform.name == "subway")
                {
                    spawnWeb = true;
                    webURL = "https://www.subway.co.jp/menu/sandwich/";
                   
                }else if(hit.transform.name == "nikugekijou")
                {
                    spawnWeb = true;
                    webURL = "https://nikudonsenmonten.com/menu1.html";
                }
            }

            for (int i = 0; i < p.Length; i++)
            {

                if (p[i] == gb)   //あたったオブジェクトと同じものを探してその配列番号を格納する
                {
                    num = i;
                    numC = true;
                }
            }
          
            if (numC == false)
            {
                num = -2;
            }
        }

        if(tap == 2)
        {
            if(Physics.Raycast(ray,out hit))
            {
                if(hit.collider.gameObject.CompareTag("info") == true)
                {
                    
                    for (int i = 0; i < p.Length; i++)
                    {
                        if (p[i] == hit.collider.gameObject)   //あたったオブジェクトと同じものを探してその配列番号を格納する
                        {
                            Destroy(p[i]);
                            pParent[i].transform.rotation = new Quaternion(0f,0f,0f,1f);
                            p[i] = null;
                            Destroy(editRend[i]);
                            Destroy(rootTex[i]);
                            scrollNum[i] = 0;
                            activate[i] = false;
                            break;
                        }
                    }
                }
            }
        }

        return num;    //クリックしたオブジェクトの配列番号を返す
    }

    void activation()  //スクロールの有効化
    {
        for (int i = 0; i < activate.Length; i++)   
        {
            if (ClickGb(tapNumFinal) == -2)
            {
                for (int j = 0; j < activate.Length; j++)
                {
                    activate[j] = false;
                }
            }

            if (ClickGb(tapNumFinal) == i)
            {
                for (int j = 0; j < activate.Length; j++)
                {
                    if (j == i)
                    {
                        activate[i] = true;
                    }
                    else if (j != i)
                    {
                        activate[j] = false;
                    }
                }

            }

            if (activate[i] == true && p[i] != null)
            {
                Scroll(i);
                Move(i);
            }
        }
    }

    void touchJudge()  //タッチ判定用関数
    {
        

        if (touch[0] == false && touch[1] == true)  //タッチの種類判定
        {
            touchOn = true;
            dontTouch = false;
            touchPosX1 = posData1.posX1;   //タッチした瞬間の座標を取得
            touchPosY1 = posData1.posY1;

            touchPosX2 = posData1.posX2;
            touchPosY2 = posData1.posY2;

            nowPosX1[0] = touchPosX1;
            nowPosY1[0] = touchPosY1;
            nowPosX1[1] = touchPosX1;
            nowPosY1[1] = touchPosY1;
        }
        else if (touch[0] == true && touch[1] == true)
        {
            touching = true;
            touchOn = false;
            dontTouch = false;

            //現在タッチしている座標
            /*    if (nowPosX1[0] == 0)
                {
                    nowPosX1[0] = touchPosX1;
                }

                if (nowPosY1[0] == 0)
                {
                    nowPosY1[0] = touchPosY1;
                }*/
            nowPosX1[0] = nowPosX1[1];
            nowPosX1[1] = a * nowPosX1[0] + (1f - a) * posData1.posX1;
            nowPosY1[0] = nowPosY1[1];
            nowPosY1[1] = a * nowPosY1[0] + (1f - a) * posData1.posY1;
            

            //    nowPosX1[1] = posData1.posX1;
            //    nowPosY1[1] = posData1.posY1;

            nowPosX2[1] = a * nowPosX2[0] + (1f - a) * posData1.posX2;
            nowPosX2[0] = nowPosX2[1];
            nowPosY2[1] = a * nowPosY2[0] + (1f - a) * posData1.posY2;
            nowPosY2[0] = nowPosY2[1];
        }
        else if (touch[0] == true && touch[1] == false)
        {
            touchUp = true;
            touching = false;
            dontTouch = false;
        }
        else if(touch[0] == false && touch[1] == false)
        {
            touchOn = false;
            touching = false;
            touchUp = false;
            dontTouch = true;
        }

        if(touchOn == true)
        {
            touchTimer = 0f;
        }
                                                           //このへんすげー冗長な書き方してる
        if (touching == true)
        {
            touchTimer += Time.deltaTime;
           
            if (posNum == 1)    //タッチしている個数が1箇所ならカーソルを移動させる
            {
                //    rotateVec = new Vector3(nowPosY1[1]-touchPosY1,nowPosX1[1]-touchPosX1,0f);
                //   cursorObject.transform.RotateAround(holoCamera.transform.position, rotateVec, 0.1f * rotateVec.magnitude * Time.deltaTime);
                //   cursorObject.transform.rotation = new Quaternion(cursorObject.transform.rotation.x, cursorObject.transform.rotation.y, 0f, cursorObject.transform.rotation.w);
                //rotateX += (nowPosY1[1] - touchPosY1) * Time.deltaTime * 0.1f;
                //rotateY += (nowPosX1[1] - touchPosX1) * Time.deltaTime * 0.1f;
                rotateX += (nowPosY1[0] - nowPosY1[1]) * Time.deltaTime * -1f;
                rotateY += (nowPosX1[0] - nowPosX1[1]) * Time.deltaTime * -1f;
               // Debug.Log(nowPosY1[0] +":"+nowPosY1[1]);

                cursorRotate.transform.rotation = Quaternion.Euler(rotateX, rotateY, 0f);

                cursorShader.material.color = new Color(29f/255f,40f/255f,178f/25f);
                  
                if (nowPosY1[1] - touchPosY1 < 1f)
                {
                    singleTapTimer += Time.deltaTime;
                }
                else
                {
                    singleTapTimer = 0f;
                }
                
            }
            else
            {
                singleTapTimer = 0f;
            }
        }

        if (touchUp == true)
        {
            if (touchTimer < 0.2f)
            {
                tapNum++;
                if(touchFin == true)
                {
                    touchFinTime = 0f;
                }
            }
            else
            {
                tapNum = 0;
            }

            touchTimer2 = 0f;
            singleTapTimer = 0f;
            //    Debug.Log(touchTimer);

            cursorShader.material.color = new Color(83f/255f, 96f/255f, 237f/255f);
            tapMove = false;

            touchFin = true;
        }

        if(dontTouch == true)
        {
            if (touchTimer2 > 0.2f)
            {
                tapNumFinal = tapNum;
                //Debug.Log(tapNumFinal);
                tapNum = 0;
                touchTimer2 = 0f;

                if(tapNumFinal == 3)   //2回タップした際にカーソルを視界の中央に戻す
                {
                    cursorRotate.transform.rotation = holoCamera.transform.rotation;
                    rotateX = holoCamera.transform.eulerAngles.x;
                    rotateY = holoCamera.transform.eulerAngles.y;
                }
            }
            else
            {
                tapNumFinal = 0;
                if(spawnWeb == true)
                {
                    SpawnWebPage(webURL);
                    spawnWeb = false;
                }
            }

            if (touchFin == true)
            {
                touchFinTime += Time.deltaTime;
                if (touchFinTime > 0.5f)
                {
                    touchFinTime = 0f;
                    touchFin = false;
                }
            }
            else
            {
                cursorRotate.transform.rotation = holoCamera.transform.rotation;
                rotateX = holoCamera.transform.eulerAngles.x;
                rotateY = holoCamera.transform.eulerAngles.y;
            }

            touchTimer2 += Time.deltaTime;
        }


        //Debug.Log(tapNum+":"+tapNumFinal+":"+touchTimer+":"+touchTimer2);
        touch[0] = touch[1];


    }
}

[System.Serializable]
public class PosData  //JSON用のオブジェクト
{
    public int posNum;
    public float posX1;
    public float posX2;
    public float posY1;  
    public float posY2;
}
