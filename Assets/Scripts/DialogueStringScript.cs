using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework.Constraints;

/* ------------------------ Main Monobehaviour -------------------- */
public class DialogueStringScript : MonoBehaviour
{
    public int[] subjectCounter; // records player choice tendencies
    public bool inTree, queuedSpecial;
    public bool options, skipScrollingInput, isScrolling;
    public Queue<QueueEntry> monologueQueue;
    public DialogueTree currentTree;
    public DialogueOption[] currentOptions;
    public MainUI ui;
    public Sprite[] images; // order these with respect to quote image enum
    private float timeInPrompt;
    private bool firstPrompt;



    [System.Serializable]
    public struct MainUI
    {
        public Image speakerImage;
        public Image panelBg;
        public TMP_Text choiceHeader;
        public TMP_Text mainMonologue;
        public ChoiceButtonUI[] choiceButtons;

        public GameObject root;
        public GameObject quoteUIRoot;
        public GameObject choiceUIRoot;
    }

    [System.Serializable]
    public struct ChoiceButtonUI
    {
        // public Image mainImage;
        public Button buttonElement;
        public TMP_Text textElement;
    }

    public void Awake()
    {
        monologueQueue = new Queue<QueueEntry>();
        subjectCounter = new int[4];
    }
    public void Update()
    {
        timeInPrompt += Time.unscaledDeltaTime;
        if (Input.GetKeyDown(KeyCode.D) && currentTree != null && !options && ( firstPrompt ? timeInPrompt > 0.5f : true)) // click D while in quote state
        {
            firstPrompt = false;
            if (isScrolling)
            {
                skipScrollingInput = true;
            }
            else{
                TransitionToNext();
            }
        }
    }

    // Updates the queue
    public void TransitionToNext()
    {
        // Sets the displays to the next quote that is queued
        if (monologueQueue.Count > 0)
        {
            QueueEntry nextInQueue = monologueQueue.Dequeue();

            if (nextInQueue.isOptions)
            {
                SetOptionGraphics(nextInQueue.options);
            }
            else
            {
                SetQuoteGraphics(nextInQueue.quote);
            }
            timeInPrompt = 0;
            AudioManager.instance.Play("Buttons");
        }

        // Flags the end of the display tree's lifecycle if there are no quotes left to display
        else
        {
            if (!queuedSpecial && currentTree.options != null)
            {
                bool result = TryQueueSpecialEvent();
                queuedSpecial = true;
                if(result) return;
            }
            Debug.Log("a");
            EndDialogueTree();
        }
    }

    // Flags the end of the display tree's lifecycle if there are no quotes left to display
    public void EndDialogueTree()
    {
        ui.root.SetActive(false);
        currentTree = null;
        inTree = false;

        if (!CinematicSystem.instance.inProgress && GameManager.instance.gameState == GameManager.GameState.DIALOGUE){
            GameManager.instance.gameState = GameManager.GameState.PRERALLY;
        }
    }

    // Initializes a new tree
    public void BeginDialogueTree(DialogueTree tree)
    {
        currentTree = tree;
        inTree = true;
        firstPrompt = true;
        queuedSpecial = false;
        GameManager.instance.gameState = GameManager.GameState.DIALOGUE;

        foreach (Monologue startQuote in tree.startQuote) { monologueQueue.Enqueue(new QueueEntry(quote: startQuote)); }

        if (!tree.noOptions) { monologueQueue.Enqueue(new QueueEntry(options: tree.options)); }

        ui.root.SetActive(true);
        TransitionToNext();
    }

    // Assigns data for the quote UI panel
    public void SetQuoteGraphics(Monologue monologue)
    {
        ui.quoteUIRoot.SetActive(true);
        ui.choiceUIRoot.SetActive(false);

        ui.mainMonologue.text = monologue.text;
        ui.speakerImage.sprite = images[(int)monologue.imageId];
        options = false;

        if (monologue.givesLoveScore)
        {
            Debug.Log("gave love?");
            GameManager.instance.GiveLoveScore(monologue.loveScore);
        }

        StartCoroutine(MonologueCoroutine(monologue));
    }

    // Assigns data for the options UI panel
    public void SetOptionGraphics(DialogueOption[] options)
    {
        currentOptions = options;
        this.options = true;

        ui.quoteUIRoot.SetActive(false);
        ui.choiceUIRoot.SetActive(true);
        ui.choiceHeader.text = currentTree.optionsHeader;
        ui.speakerImage.sprite = images[(int)options[0].imageId];

        for (int i = 0; i < options.Length; i++)
        {
            ChoiceButtonUI button = ui.choiceButtons[i];
            DialogueOption option = options[i];

            button.textElement.text = "-> " + option.text;
        }
    }

    // Queues the rest of the monologues specific to that option
    public void ChooseOption(int option)
    {

        subjectCounter[(int)currentOptions[option].optionType]++;
        foreach (Monologue monologue in currentTree.responses[option])
        {
            monologueQueue.Enqueue(new QueueEntry(quote: monologue));
        }

        if (currentOptions[option].optionType == OptionType.INSULTNED) { GameManager.instance.InsultNed(); }
        currentOptions = null;

        // TryQueueSpecialEvent();
        TransitionToNext();
    }

    // Queues a special text upon a special condition at the end of the tree if nessecary
    public bool TryQueueSpecialEvent()
    {
        if(subjectCounter[(int)OptionType.INSULTNED] == 3){ 
            monologueQueue.Enqueue(new QueueEntry(Dialogue.specialNedInsult[0])); 
            monologueQueue.Enqueue(new QueueEntry(Dialogue.specialNedInsult[1])); 
            return true;
        }
        if(subjectCounter[(int)OptionType.RIZZ] == 3 && GameManager.instance.lovePointsThisTree > 0){ 
            monologueQueue.Enqueue(new QueueEntry(Dialogue.specialCharismatic[Random.Range(0, 2)])); 
            monologueQueue.Enqueue(new QueueEntry(Dialogue.specialCharismatic[Random.Range(2, 4)])); 
            return true;
        }
        if (subjectCounter[(int)OptionType.ROAST] == 3 && GameManager.instance.lovePointsThisTree > 0)
        {

            if (Random.Range(0, 2) == 1)
            {
                monologueQueue.Enqueue(new QueueEntry(Dialogue.specialRoast[Random.Range(0, 2)]));
            }
            else
            {
                if (Random.Range(0, 2) == 1)
                {
                    monologueQueue.Enqueue(new QueueEntry(Dialogue.specialRoast[2]));
                }
                else
                {
                    monologueQueue.Enqueue(new QueueEntry(Dialogue.specialRoast[3]));
                    monologueQueue.Enqueue(new QueueEntry(Dialogue.specialRoast[4]));
                }
            }
            return true;
        }
        
        return false;
    }


    public IEnumerator MonologueCoroutine(Monologue monologue, int lettersPerSecond = 15)
    {
        ui.mainMonologue.text = "";
        isScrolling = true;

        for (int i = 0; i < ui.mainMonologue.text.Length; i++)
        {
            if (skipScrollingInput) { skipScrollingInput = false; break; }
            ui.mainMonologue.text += monologue.text[i];
            yield return new WaitForSecondsRealtime(1.0f / lettersPerSecond);
        }

        isScrolling = false;
        ui.mainMonologue.text = monologue.text;    
    }
}

/* ------------------------ Supporting Classes and Data Structures -------------------- */

// Class used as a wrapper to contain the data of quotes in queue for display, UI, and functionality 
public class QueueEntry
{
    public Monologue quote;
    public DialogueOption[] options;
    public bool isOptions { get { return options != null; } }

    public QueueEntry(Monologue quote = null, DialogueOption[] options = null)
    {
        this.quote = quote;
        this.options = options;
    }
}

// Main Dialogue tree class representation
[System.Serializable]
public class DialogueTree
{
    public Monologue[] startQuote;
    public DialogueOption[] options;
    public string optionsHeader;
    public bool noOptions {get{ return options == null; }}
    public Monologue[/* option choice index */][ /* Response count */ ] responses;

    public DialogueTree(Monologue startQuote, DialogueOption[] options, Monologue[][] responses, string optionsHeader = "HOW DO YOU RESPOND?")
    {
        this.startQuote = new Monologue[] { startQuote };
        this.options = options;
        this.responses = responses;
        this.optionsHeader = optionsHeader;
    }
    public DialogueTree(Monologue[] startQuote)
    {
        this.startQuote = startQuote;
        options = null;
        responses = null;
        optionsHeader = "";
    }
}

// Base class to represent quotes, used for any NPC speech in dialogue trees
[System.Serializable]
public class Monologue
{
    public string text;
    public int loveScore;
    public QuoteImage imageId;
    public bool givesLoveScore { get { return loveScore > 0; } }
    public bool insultsNed;

    public Monologue(string text, QuoteImage imageId, int loveScore = 0, bool insultsNed = false)
    {
        this.text = text;
        this.imageId = imageId;
        this.loveScore = loveScore;
        this.insultsNed = insultsNed;
    }
}

// Class to represent and display options and choices to the player in dialogue trees 
[System.Serializable]
public class DialogueOption : Monologue
{
    public OptionType optionType;

    public DialogueOption(string text, QuoteImage imageId, OptionType optionType, int loveScore = 0) : base(text, imageId, loveScore)
    {
        this.optionType = optionType;
    }
}

// Enum to map images with their name/intention, indexed and mapped in the main monobehavour class
public enum QuoteImage
{
    NONE,

    // Lily expressions 
    LILYFlatteredImage,
    LILYDissapointed,

    // Jequevonte expressions 
    JEQUEVONTEUpsetImage,
    JEQUEVONTEProudImage,
    JEQUEVONTEIdleImage,

    // Bartholemew expressions 
    BARTHOLEMEWProudImage,
    BARTHOLEMEWIdleImage,
    BARTHOLEMEWdissapointedImage,

    // Ned expressions 
    NEDInsultedImage,
    NEDNormal,

}

// The different type of options a player can choose
public enum OptionType
{
    ROAST,
    RIZZ,
    REDEMPTION,
    INSULTNED,
}

/* ------------------------ Dialogue Tree Data -------------------- */
public static class Dialogue
{
    public static DialogueTree[] winTrees = new DialogueTree[6]
    {
        /* --- Dialogue tree 1 --- */
        new DialogueTree(
            new Monologue("LILY: Nice one Barty! Now, entertain me and tell me a joke!", QuoteImage.LILYFlatteredImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Why did the chicken cross the road? TO GET TO THE OTHER SIDE LOLOLOL", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new DialogueOption("Jequevonte serves like he's scared of being swatted!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new DialogueOption("Do I need to? Ned’s existence is a joke XD", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: That joke was ass.", QuoteImage.LILYDissapointed)
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: LOLOLOL you kinda funny twin. Gj.", QuoteImage.LILYFlatteredImage, loveScore: 10)
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("LILY: ...", QuoteImage.LILYDissapointed), // Will always be insulting ned
                    new Monologue("Ned: ...", QuoteImage.NEDInsultedImage)
                }
            }
        ),

        /* --- Dialogue tree 2 --- */
        new DialogueTree(
            new Monologue("JEQUEVONTE: You just got lucky! I’m sure to win this next one!", QuoteImage.JEQUEVONTEProudImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Quit buzzing so loud and prove it!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new DialogueOption("Pfft, you wish..! I’ll win this one just like I’m winning the Queen!", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new DialogueOption("Even if you and Ned teamed up against me, you’d still be a bunch of LOSERS!", QuoteImage.BARTHOLEMEWProudImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: Hmm..you can be quite charming when you get fiesty. I like that.", QuoteImage.LILYFlatteredImage, loveScore: 10),
                    new Monologue("...", QuoteImage.JEQUEVONTEUpsetImage)
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: ..Was that a joke as well? NOBODY 'wins' me. ", QuoteImage.LILYDissapointed),
                    new Monologue("NED: and thats on PERIODT ", QuoteImage.NEDNormal)
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("LILY: Super confident I see... charming :3", QuoteImage.LILYFlatteredImage, loveScore: 5), // Will always be insulting ned
                    new Monologue("NED: -_- ", QuoteImage.NEDInsultedImage) // Will always be insulting ned
                }
            }
        ),

        /* --- Dialogue tree 3 --- */
        new DialogueTree(
            new Monologue("NED: C’mon guys, don’t you think that’s enough for now? I’m feeling real beat...", QuoteImage.NEDInsultedImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Pshh, that’s nothing compared to how I'mma beat Jequevonte this round!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new DialogueOption("It’s never enough if its for Queen Lily!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new DialogueOption("Shut up Ned. This isn't about you!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: I like my men feisty but I don’t like them pretentious. That’s MY thing!", QuoteImage.LILYDissapointed),
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: How dedicated you are...how...adorable. Hehe. I just want to wrap you up in my web and eat you!", QuoteImage.LILYFlatteredImage, loveScore: 10),
                    new Monologue("...", QuoteImage.JEQUEVONTEUpsetImage)
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("NED: ... ", QuoteImage.NEDInsultedImage) // Will always be insulting ned
                }
            }
        ),
        
        /* --- Dialogue tree 4 --- */
        new DialogueTree(
            new Monologue("LILY: To be honest, it’s been fun but I’m kind of getting bored here... Hey. Whats your favourite color?", QuoteImage.LILYFlatteredImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Whatever colour you like is my favourite, Lily.", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new DialogueOption("Whatever colour my enemies bleed. #edgy #edgelord #FaZe", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new DialogueOption("I like the colour that Ned’s about to turn when I’m done with him!", QuoteImage.BARTHOLEMEWProudImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: I like my men feisty but I don’t like them pretentious. That’s MY thing!", QuoteImage.LILYDissapointed),
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: How dedicated you are...how...adorable. Hehe. I just want to wrap you up in my web and eat you!", QuoteImage.LILYFlatteredImage, loveScore: 10),
                    new Monologue("...", QuoteImage.JEQUEVONTEUpsetImage),
                    new Monologue("...please don't.", QuoteImage.JEQUEVONTEUpsetImage)
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("LILY: ... ", QuoteImage.LILYDissapointed), // Will always be insulting ned
                    new Monologue("NED: ...relax...please.. ", QuoteImage.NEDInsultedImage) // Will always be insulting ned
                }
            }
        ),

        /* --- Dialogue tree 5 --- */
        new DialogueTree(
            new Monologue("JEQUEVONTE: You may have beaten me all those other times, but this time I’ll get you real good!!! Lily is Mine!!", QuoteImage.JEQUEVONTEProudImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("As if! I could beat you 6-7 more times if I wanted to.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new DialogueOption("*You shush opponent, look at Lily and slide your finger along your jawline (rizz)*", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new DialogueOption("The only thing getting beat here other than you is Ned - C'MERE NED", QuoteImage.BARTHOLEMEWProudImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: Six..Seven..really? Euugh brotha eeugh.", QuoteImage.LILYDissapointed),
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: Go on! My lovebug <3 <3 !!11!1", QuoteImage.LILYFlatteredImage, 10),
                    new Monologue("JEQUEVONTE: ...", QuoteImage.JEQUEVONTEUpsetImage)
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("NED: PLEASE...no..", QuoteImage.NEDNormal), // Will always be insulting ned
                    new Monologue("NED: ..no..more..", QuoteImage.NEDInsultedImage),
                    new Monologue("BARTHOLEMEW: HEHEHE BALL AINT GONNA SERVE ITSELF-", QuoteImage.BARTHOLEMEWProudImage) // Will always be insulting ned
                }
            }
        ),

        /* --- Dialogue tree 6 --- */
        new DialogueTree(
            new Monologue("Looks like the game is almost over... what a shame, I was just beginning to have fun!", QuoteImage.LILYDissapointed), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Don’t worry Queen Lily, once this is over I can show you what real fun is! ;D", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new DialogueOption("Don’t be sad, I was just warming up! Watch this!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new DialogueOption("If Ned would stop complaining, I’d do this all day!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: ...Disgusting.", QuoteImage.LILYDissapointed),
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: Oh! Well, how thrilling and exciting then.. Show me", QuoteImage.LILYFlatteredImage)
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("Ned:...Let it all be over soon..please..", QuoteImage.NEDInsultedImage)
                }
            }
        ),

    };

    public static DialogueTree[] loseTrees = new DialogueTree[6]
    {
        /* --- Dialogue tree 1 --- */
        new DialogueTree(
            new Monologue("JEQUEVONTE: Bro Lily don’t want you and not even trash would at this point.", QuoteImage.JEQUEVONTEProudImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("Keep talking and maybe someone will mistake that noise for actual buzz.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new ("Maybe not trash, but it looks like you can’t stop picking me up hehe", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new ("If by trash you mean Ned, then thats cool with me", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED) // Will always be insulting ned
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("JEQUAVONTE: Aren't you one to talk?", QuoteImage.JEQUEVONTEProudImage),
                    new ("LILY: ..He's kinda right, Bartholomew..", QuoteImage.LILYDissapointed),
                },
                new Monologue[]{ // Option 2 responses
                    new ("LILY: OMG!!11??1 Enemies to lovers??", QuoteImage.LILYFlatteredImage, 10),
                    new ("JEQUEVONTE: ...", QuoteImage.JEQUEVONTEUpsetImage)
                },
                new Monologue[]{ // Option 3 responses
                    new ("JEQUEVONTE: sure...", QuoteImage.JEQUEVONTEIdleImage),
                    new ("NED: ...", QuoteImage.NEDInsultedImage),
                }
            }
        ),
        
        /* --- Dialogue tree 2 --- */
        new DialogueTree(
            new Monologue("NED: Damn bro you got beat harder than I am LOL", QuoteImage.NEDNormal), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("Save that for Jequavonte as I walk home with my girl", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new ("Lily can beat me any day of the week.", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new ("Ned you clearly don't own an air fryer.", QuoteImage.BARTHOLEMEWdissapointedImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("JEQUEVONTE: Lets see about that!", QuoteImage.JEQUEVONTEProudImage),
                    new ("LILY: How cute.. I’d love to take you to my...house... and wrap you up in my little web!", QuoteImage.LILYFlatteredImage, 10)
                },
                new Monologue[]{ // Option 2 responses
                    new ("LILY: While I love a good hunt, you should probably talk to someone about that... Like, a professional you know?", QuoteImage.LILYDissapointed),
                },
                new Monologue[]{ // Option 3 responses
                    new ("NED: ...,", QuoteImage.NEDNormal),
                    new ("NED: ...how dare you.", QuoteImage.NEDInsultedImage)
                }
            }
        ),

        /* --- Dialogue tree 3 --- */
        new DialogueTree(
            new Monologue("LILY: What are you doing? Do you even want me???", QuoteImage.LILYDissapointed), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("Of course! I want you more than anything else!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new ("The only other thing I want is to DESTROY Jequavonté!!!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new ("I just really want to beat Ned’s stupid face up, tbh.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("LILY: Then PROVE it and stop wasting my time, scum.", QuoteImage.LILYDissapointed)
                },
                new Monologue[]{ // Option 2 responses
                    new ("LILY: I like your viciousness... prove  to me that your buzz has a bite!", QuoteImage.LILYFlatteredImage, 5),
                },
                new Monologue[]{ // Option 3 responses
                    new ("JEQUEVONTE: Fair enough.", QuoteImage.JEQUEVONTEIdleImage),
                    new ("NED: Man..why..?", QuoteImage.NEDInsultedImage)
                }
            }
        ),

        /* --- Dialogue tree 4 --- */
        new DialogueTree(
            new Monologue("JEQUEVONTE: All talk. no game.", QuoteImage.JEQUEVONTEIdleImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("You wouldn’t know game even if it looked at you in your beady little eyes.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new ("The only game I want to play is the game of love", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new ("It’s all Ned’s fault, he’s so useless and dumb.", QuoteImage.BARTHOLEMEWdissapointedImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("JEQUEVONTE: ...We ALL have beady eyes. We’re bugs. Idiot.", QuoteImage.JEQUEVONTEIdleImage)
                },
                new Monologue[]{ // Option 2 responses
                    new ("LILY: How disgustingly cheesy. Good thing I like my prey topped with cheese though!", QuoteImage.LILYFlatteredImage, 10),
                    new ("LILY: ...oops, did I say that out loud?", QuoteImage.LILYFlatteredImage)
                },
                new Monologue[]{ // Option 3 responses
                    new ("NED: ... ", QuoteImage.NEDInsultedImage)
                }
            }
        ),

        /* --- Dialogue tree 5 --- */
        new DialogueTree(
            new Monologue("NED: When it comes to flies, you seem pretty weak not gonna lie...", QuoteImage.NEDNormal), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("Me? Weak? I was just holding back!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new ("The only thing I’m weak for is Queen Lily!", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new ("WHO DO YOU THINK YOU ARE? YOU'RE JUST A LOUSY TOAD. WHAT EVEN ARE YOU. A TOAD OR A FROG? ALSO WHY ARE YOU EVEN SO VULUPTUOUS FOR?? WHY ARE YOU SO ROUND? WHAT'S WRONG WITH YOU?" +
                    " ALSO WHAT'S WITH FROGS? THEY'RE JUST AMPHIBIANS BUT WHAT EVEN ARE AMPHIBIANS? THEY SURVIVE ON LAND AND IN WATER, LIKE WTF? PICK A SIDE, WEIRDO. YOU DON'T BELOND ANYWHERE. WHAT KIND " +
                    "OF ALIEN ARE YOU? YOU DON'T EVEN BELONG HERE. AND THEN YOU HAVE THE GALL TO TAKE LILY FROM ME!? AS IF SHE'D WANT SOME STUPID TOAD-FROG-ALIEN-BALL THING LIKE YOU. DON'T MAKE ME LAUGH, " +
                    "YOU'RE HONESTLY SO PATHETIC THINKING YOU CAN GET TO ME. NO, YOU DIDN'T GET TO ME AT ALL! WHY WOULD YOU THINK THAT? WHAT ARE--", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("JEQUEVONTE: Holding back from what? Crying from your loss? Hahahaha", QuoteImage.JEQUEVONTEProudImage)
                },
                new Monologue[]{ // Option 2 responses
                    new ("LILY: Oh, I just love it when they grovel! So pathetically delicious.", QuoteImage.LILYFlatteredImage, 10),
                },
                new Monologue[]{ // Option 3 responses
                    new ("NED: Damn bro you did not need to say all that.", QuoteImage.NEDInsultedImage),
                    new ("YOU: Yes I did.", QuoteImage.BARTHOLEMEWIdleImage)
                }
            }
        ),

        /* --- Dialogue tree 6 --- */
        new DialogueTree(
            new Monologue("LILY: How pathetic, I guess Jequavonte might be the better mosquito after all.", QuoteImage.LILYDissapointed), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("Jequavnte aint seen nothin yet", QuoteImage.BARTHOLEMEWIdleImage, OptionType.REDEMPTION),
                new ("please. He cant even keep the ball up for half a minute", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new ("Me? Pathetic? What about Ned?\nLook at him. Just look.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("JEQUEVONTE: I see...", QuoteImage.JEQUEVONTEIdleImage),
                    new ("LILY: I dont know about that one", QuoteImage.LILYDissapointed)
                },
                new Monologue[]{ // Option 2 responses
                    new ("Damn, Ok..", QuoteImage.LILYFlatteredImage, 10),
                },
                new Monologue[]{ // Option 3 responses
                    new ("NED: ...", QuoteImage.NEDInsultedImage)
                }
            }
        )
    };

    public static DialogueTree[] introTrees = new DialogueTree[]
    {
        new DialogueTree(
            new Monologue[] {
                new("JEQUEVONTE: Hey...", QuoteImage.JEQUEVONTEIdleImage),
                new("BARTHOLEMEW: What's going on here!??", QuoteImage.BARTHOLEMEWIdleImage),
                new("JEQUEVONTE: Lily, why are you--", QuoteImage.JEQUEVONTEIdleImage),
                new("BARTHOLEMEW: --with HIM!!??", QuoteImage.BARTHOLEMEWdissapointedImage),

                new("LILY: I- oh dear. Hi boys... I can explain...", QuoteImage.LILYDissapointed),
                new("NED: Uhm..So is that a yes? :D", QuoteImage.NEDNormal),
                new("LILY: Ned-", QuoteImage.LILYDissapointed),

                new("BARTHOLEMEW: Hold on..why are YOU here as well? I'm the one dating Lily here!", QuoteImage.BARTHOLEMEWIdleImage),
                new("JEQUEVONTE: What? That's MY line! I'm the one who's dating Lily!", QuoteImage.JEQUEVONTEIdleImage),
                new("NED: What?? But I thought Lily was single!", QuoteImage.NEDInsultedImage),

                new("BARTHOLEMEW: Stay out of this you wierd...voluptuous toad!!!", QuoteImage.BARTHOLEMEWIdleImage),
                new("NED: Hello? I'm a frog first of all and my name is Ned! I don't even know who you freaks are!! I'm just here for the beautiful and mesmerizing Queen Lily!", QuoteImage.NEDNormal),
                new("JEQUEVONTE: Well you can't have her! She's MINE!", QuoteImage.JEQUEVONTEIdleImage),
                new("BARTHOLEMEW: Excuse you, she's MINE actually!", QuoteImage.BARTHOLEMEWIdleImage),
                new("JEQUEVONTE: No she's--", QuoteImage.JEQUEVONTEIdleImage),
                new("LILY: ENOUGH!!", QuoteImage.LILYDissapointed),
                new("BARTHOLEMEW: ...", QuoteImage.BARTHOLEMEWIdleImage),
                new("JEQUEVONTE: ...", QuoteImage.JEQUEVONTEIdleImage),
                new("Ned: ...", QuoteImage.NEDNormal),
                new("LILY: If you want to have me for yourself then show me that you're worthy enough to keep me. And I know just the way!", QuoteImage.LILYFlatteredImage),
                new("Ned: ...", QuoteImage.NEDInsultedImage),
                new ("Ned: Haha... why are you looking at me like that Lily??", QuoteImage.NEDNormal),
            }
        ),

        new DialogueTree(
            new Monologue[] {
                new("NED: ...Ok I did NOT consent to this.", QuoteImage.NEDInsultedImage),
                new("LILY: For one of you to win my loyalty and love, one of you two bugs need to win this game of Bump Frog and win my heart at the same time!", QuoteImage.LILYFlatteredImage),
                new("BARTHOLEMEW: Oh you got it! I'll get this easy peezy!", QuoteImage.BARTHOLEMEWProudImage),
                new("JEQUEVONTE: Bring it on!!!", QuoteImage.JEQUEVONTEProudImage)
            }
        )
    };

    public static DialogueTree[] outroTrees = new DialogueTree[]
    {

        new DialogueTree(
            new Monologue[] {

                new("LILY: Well...I have to admit. You’re pretty impressive Barty. Winning or losing.", QuoteImage.LILYFlatteredImage),
                new("BARTHOLEMEW: Heh I have my moments. But I’d never let Jequavonte or that...weird ball guy get in my way when it comes to you!!", QuoteImage.BARTHOLEMEWProudImage),
                new("NED: Hello? I have a name too! Y’know, Ned? Why does everybody treat me like this?!", QuoteImage.NEDNormal),
                new("BARTHOLEMEW: Shut up Ned.", QuoteImage.BARTHOLEMEWIdleImage),
                new("JEQUEVONTE: Yeah Ned, keep out of this. ", QuoteImage.JEQUEVONTEIdleImage),
                new("NED: I...why am I even still here. I’m out!! Lily’s not even that pretty anyways, I’ll find a new pond to hop in! Forget you freaks!!", QuoteImage.NEDInsultedImage),
                new("*Ned disappears* (So the ball like vanishes or something)", QuoteImage.NONE),
                new("LILY: Well anyways, watching you guys battle has made me quite...hungry. And Barty, you’ve impressed me so much today that from now on, there will be no more competition keeping you from me. ", QuoteImage.LILYFlatteredImage),
                new("BARTHOLEMEW: ...", QuoteImage.BARTHOLEMEWProudImage),
                new("JEQUEVONTE: What is...that supposed to mean?", QuoteImage.JEQUEVONTEUpsetImage),
                new("LILY: Well...let me show you!", QuoteImage.LILYFlatteredImage),
                new("JEQUEVONTE: ...Hey...I don’t like this... What are you--", QuoteImage.NONE),
                new("AAAAAAAAAAAAAHHHHHH", QuoteImage.NONE),
            }
        ),

        new DialogueTree(
            new Monologue[] {
                new("JEQUEVONTE: Well that was easy-peezy. Were you even trying? Win or lose against me, it doesn’t matter if you can’t win Lily’s heart.", QuoteImage.JEQUEVONTEProudImage),
                new("LILY: Well this has been...", QuoteImage.LILYDissapointed),
                new("Hmm...", QuoteImage.LILYDissapointed),
                new("Rather disappointing if I’ll admit. I was rooting for you, Barty.", QuoteImage.LILYDissapointed),
                new("NED: This is too embarrassing. I can’t watch this!! ", QuoteImage.NEDInsultedImage),
                new("*Ned disappears* ", QuoteImage.NONE),
                new("BARTHOLEMEW: I’m sorry! I don’t know what went wrong, I don’t think my mind was in the game. ", QuoteImage.BARTHOLEMEWdissapointedImage),
                new("LILY: Hm. Clearly not. You’re more useless than a maggot. ", QuoteImage.LILYDissapointed),
                new("BARTHOLEMEW: I promise I’m not! Is there any way I can be of use to you? I can’t lose you Lily!", QuoteImage.BARTHOLEMEWdissapointedImage),
                new("LILY: ...", QuoteImage.LILYDissapointed),
                new("Well...", QuoteImage.LILYDissapointed),
                new("There is one thing I can think of to put you to use...", QuoteImage.LILYFlatteredImage),
                new("JEQUEVONTE: Uh oh...", QuoteImage.JEQUEVONTEProudImage),
                new("BARTHOLEMEW: What is it? I’ll do anything!", QuoteImage.BARTHOLEMEWIdleImage),
                new("LILY: Come closer and I’ll tell you...", QuoteImage.LILYFlatteredImage),
                new("...", QuoteImage.BARTHOLEMEWIdleImage),
                new("Closer...", QuoteImage.LILYDissapointed),
                new("BARTHOLEMEW: Wait what’s going on? ", QuoteImage.NONE),
                new("No..no... Get away!", QuoteImage.NONE),
                new("NOOOOOOOOOOOOOO", QuoteImage.NONE)
            }
        ),

        new DialogueTree(
            new Monologue[] {
                new("NED: ...", QuoteImage.NEDInsultedImage),
                new("You really shouldn’t have done that..", QuoteImage.NEDInsultedImage),
                new("BARTHOLEMEW: What is this? Whats happening??", QuoteImage.NEDInsultedImage),
                new("LILY: Oh no.. This is why you should’ve just focused on me! Not that dumb frog!", QuoteImage.NEDInsultedImage),
                new("JEQUEVONTE: This can’t be happening.", QuoteImage.NEDInsultedImage),
                new("NED: I’ve had enough of this. You’ve crossed the line and now you shall pay for your sins...", QuoteImage.NEDInsultedImage),
                new("BARTHOLEMEW: What are you... ", QuoteImage.NEDInsultedImage),
                new("AAAAAAAAAA", QuoteImage.NEDInsultedImage)
            }
        )
    };

    public static Monologue[] specialCharismatic = new Monologue[]{
        new("LILY: My my, look who’s a charmer! You’re on a roll today aren’t you? I wouldn’t expect any less.", QuoteImage.LILYFlatteredImage),
        new("LILY: Aaw, you’re adorable. I just want to sink my teeth into you!!", QuoteImage.LILYFlatteredImage),

        new("JEQUEVONTE: Yeah, yeah we get it! Didn’t know we had Prince Charming over here...", QuoteImage.JEQUEVONTEIdleImage),
        new("JEQUEVONTE: Hey, I'll shut your mouth real soon!! The Queen won't fall for your cheap tricks!!", QuoteImage.JEQUEVONTEIdleImage)
    };

    public static Monologue[] specialNedInsult = new Monologue[]{
        new("LILY: I know they say to keep your eyes on the ball but the only person you should keep your eyes on is me!  Pay attention to me, not that...voluptuous frog.", QuoteImage.LILYDissapointed),
        new("NED: One day you will have to answer for your actions.\nAnd god may not be so... merciful.", QuoteImage.NEDInsultedImage)
        
        // new("LILY: Such a bore...! Are you sure you’re going against the right person here?", QuoteImage.LILYFlatteredImage),
    };

    public static Monologue[] specialRoast = new Monologue[]{
        new("LILY: Oh my lovelies, I just ADORE it when you fight like this. What a feast for the eyes.", QuoteImage.LILYFlatteredImage),
        new("LILY: Nothing gets me more riled up than watching you little bugs fight!", QuoteImage.LILYFlatteredImage),

        new("NED: WOO GET HIS ASS! (and not mine pls) ", QuoteImage.NEDNormal),
        new("NED: DAMN sick burn. So how about, uh, letting me go?", QuoteImage.NEDNormal),
        new("BARTHOLEMEW: Nah.", QuoteImage.BARTHOLEMEWProudImage)
    };
    // public static
}