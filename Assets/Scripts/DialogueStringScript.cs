using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/* ------------------------ Main Monobehaviour -------------------- */
public class DialogueStringScript : MonoBehaviour
{
    public int[] subjectCounter; // records player choice tendencies
    public bool inTree;
    public bool options;
    public Queue<QueueEntry> monologueQueue;
    public DialogueTree currentTree;
    public DialogueOption[] currentOptions;
    public MainUI ui;
    public Sprite[] images; // order these with respect to quote image enum
    
    

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
        if (Input.anyKeyDown && currentTree != null && !options)// clicked anything while in quote state
        {
            TransitionToNext();
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
            AudioManager.instance.Play("Buttons");
        }

        // Flags the end of the display tree's lifecycle if there are no quotes left to display
        else
        {
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
        GameManager.instance.gameState = GameManager.GameState.PRERALLY;
    }

    // Initializes a new tree
    public void BeginDialogueTree(DialogueTree tree)
    {
        currentTree = tree;
        inTree = true;
        GameManager.instance.gameState = GameManager.GameState.DIALOGUE;
        monologueQueue.Enqueue(new QueueEntry(quote: tree.startQuote));
        monologueQueue.Enqueue(new QueueEntry(options: tree.options));

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
    }

    // Assigns data for the options UI panel
    public void SetOptionGraphics(DialogueOption[] options)
    {
        currentOptions = options;
        this.options = true;

        ui.quoteUIRoot.SetActive(false);
        ui.choiceUIRoot.SetActive(true); 
        ui.choiceHeader.text = currentTree.optionsHeader; 

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
        currentOptions = null;

        TryQueueSpecialEvent();
        TransitionToNext();
    }

    // Queues a special text upon a special condition at the end of the tree if nessecary
    public void TryQueueSpecialEvent()
    {
        // if(subjectCounter[(int)OptionType.INSULTNED] == 3){ monologueQueue.Enqueue(); return;}
        // if(subjectCounter[(int)OptionType.INSULTNED] == 3){ monologueQueue.Enqueue(); return;}
        // if(subjectCounter[(int)OptionType.INSULTNED] == 3){ monologueQueue.Enqueue(); return;}
        // if(subjectCounter[(int)OptionType.INSULTNED] == 3){ monologueQueue.Enqueue(); return;}
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
    public Monologue startQuote;
    public DialogueOption[] options;
    public string optionsHeader;
    public Monologue[/* option choice index */][ /* Response count */ ] responses;

    public DialogueTree(Monologue startQuote, DialogueOption[] options, Monologue[][] responses, string optionsHeader = "HOW DO YOU RESPOND?")
    {
        this.startQuote = startQuote;
        this.options = options;
        this.responses = responses;
        this.optionsHeader = optionsHeader;
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
            new Monologue("LILY: To start the game off in good spirits, I want you to tell me a joke", QuoteImage.JEQUEVONTEProudImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Why did the chicken cross the road? to get to the other side", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new DialogueOption("Jequevonte serves like he's scared of being swatted!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new DialogueOption("Do I need to? Ned’s existence is a joke hahaha", QuoteImage.NEDInsultedImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: That joke was ass.", QuoteImage.LILYFlatteredImage)
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: LOLOLOL you kinda funny twin", QuoteImage.JEQUEVONTEUpsetImage, loveScore: 10)
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("LILY: ...", QuoteImage.NEDInsultedImage) // Will always be insulting ned
                }
            }
        ),

        /* --- Dialogue tree 2 --- */
        new DialogueTree(
            new Monologue("JEQUEVONTE: You just got lucky! I’m sure to win this next one!", QuoteImage.JEQUEVONTEProudImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Quit buzzing so loud and prove it!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new DialogueOption("Pfft, you wish..! I’ll win this one just like I’m winning the Queen!", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
                new DialogueOption("Even if you and Ned teamed up against me, you’d still be a bunch of LOSERS!", QuoteImage.NEDInsultedImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: Hmm..you can be quite charming when you get fiesty. I like that.", QuoteImage.LILYFlatteredImage, loveScore: 10),
                    new Monologue("...", QuoteImage.JEQUEVONTEUpsetImage)
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: ..Was that a joke as well? NOBODY 'wins' me. ", QuoteImage.LILYFlatteredImage),
                    new Monologue("NED: and thats on PERIODT ", QuoteImage.NEDNormal)
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("LILY: Super confident I see... charming :)", QuoteImage.LILYFlatteredImage, loveScore: 5), // Will always be insulting ned
                    new Monologue("NED: -_- ", QuoteImage.NEDInsultedImage) // Will always be insulting ned
                }
            }
        ),

        /* --- Dialogue tree 3 --- */
        new DialogueTree(
            new Monologue("NED: C’mon guys, don’t you think that’s enough for now? I’m feeling real beat...", QuoteImage.NEDInsultedImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Pshh, that’s nothing compared to how Imma beat Jequevonte this round!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new DialogueOption("It’s never enough if its for Queen Lily!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new DialogueOption("Shut up Ned. This isn't about you!", QuoteImage.NEDInsultedImage, OptionType.INSULTNED)
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
            new Monologue("LILY: To be honest, it’s been fun but I’m kind of getting bored here...\n...\nHey. Whats your favourite color?", QuoteImage.LILYDissapointed), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Whatever colour you like is my favourite, Lily.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new DialogueOption("Whatever colour my enemies bleed.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new DialogueOption("I like the colour that Ned’s about to turn when I’m done with him!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
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
                    new Monologue("LILY: ... ", QuoteImage.LILYDissapointed), // Will always be insulting ned
                    new Monologue("NED: pause... ", QuoteImage.NEDInsultedImage) // Will always be insulting ned
                }
            }
        ),

        /* --- Dialogue tree 5 --- */
        new DialogueTree(
            new Monologue("JEQUEVONTE: You may have beaten me all those other times, but this time I’ll get you real good!!! Lily is Mine!!", QuoteImage.JEQUEVONTEProudImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("As if! I could beat you 6-7 more times if I wanted to.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new DialogueOption("*Shush opponent, look at Lily, Slide your finger along your jawline (rizz)", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new DialogueOption("The only thing getting beat here other than you is Ned - C'MERE NED", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new Monologue("LILY: Go on! My lovebug", QuoteImage.LILYFlatteredImage, 10),
                    new Monologue("JEQUEVONTE...", QuoteImage.JEQUEVONTEUpsetImage)
                },
                new Monologue[]{ // Option 2 responses
                    new Monologue("LILY: Six..Seven..really? Lame.", QuoteImage.LILYDissapointed),
                },
                new Monologue[]{ // Option 3 responses
                    new Monologue("NED: please no more...", QuoteImage.NEDInsultedImage), // Will always be insulting ned
                    new Monologue("BARTHOLEMEW: BALL AINT GONNA SERVE ITSELF-", QuoteImage.BARTHOLEMEWProudImage) // Will always be insulting ned
                }
            }
        ),

        /* --- Dialogue tree 6 --- */
        new DialogueTree(
            new Monologue("Looks like the game is almost over... what a shame, I was just beginning to have fun!", QuoteImage.LILYDissapointed), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new DialogueOption("Don’t worry Queen Lily, once this is over I can show you what real fun is", QuoteImage.BARTHOLEMEWProudImage, OptionType.RIZZ),
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
                new ("Maybe not trash, but it looks like you can’t stop picking me up hehe", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new ("If by trash you mean Ned, then thats cool with me", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED) // Will always be insulting ned
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("Aren't you one to talk?", QuoteImage.JEQUEVONTEProudImage),
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
            new Monologue("Damn bro you got beat harder than I am LOL", QuoteImage.NEDNormal), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("Lily can beat me any day of the week.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new ("Save that for Jequavonte as I walk home with my girl", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new ("Ned you clearly don't own an air fryer.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("JEQUEVONTE: Lets see about that!", QuoteImage.JEQUEVONTEProudImage),
                    new ("LILY: How cute.. I’d love to take you to my...house... and wrap you up in my little web!", QuoteImage.LILYFlatteredImage)
                },
                new Monologue[]{ // Option 2 responses
                    new ("LILY: While I love a good hunt, you should probably talk to someone about that... Like, a professional you know?", QuoteImage.LILYDissapointed),
                },
                new Monologue[]{ // Option 3 responses
                    new ("NED: ...how dare you", QuoteImage.NEDInsultedImage)
                }
            }
        ),

        /* --- Dialogue tree 3 --- */
        new DialogueTree(
            new Monologue("LILY: What are you doing? Do you even want me???", QuoteImage.LILYDissapointed), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("Of course! I want you more than anything else!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new ("LILY: The only other thing I want is to DESTROY Jequavonté!!!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new ("I just really want to beat Ned’s stupid face up tbh.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
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
                    new ("NED: why :(", QuoteImage.NEDInsultedImage)
                }
            }
        ),

        /* --- Dialogue tree 4 --- */
        new DialogueTree(
            new Monologue("JEQUEVONTE: All talk. no game.", QuoteImage.JEQUEVONTEIdleImage), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("You wouldn’t know game even if it looked at you in your beady little eyes.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.ROAST),
                new ("The only game I want to play is the game of love", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new ("It’s all Ned’s fault, he’s so useless and dumb.", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
            },

            new Monologue[][]{ // List of responses depending on option chosen
                
                new Monologue[]{ // Option 1 responses
                    new ("JEQUEVONTE: ...We ALL have beady eyes. We’re bugs. Idiot.", QuoteImage.JEQUEVONTEIdleImage)
                },
                new Monologue[]{ // Option 2 responses
                    new ("Lily: How disgustingly cheesy. Good thing I like my prey topped with cheese though. Oops, did I say that out loud?", QuoteImage.LILYFlatteredImage, 10),
                },
                new Monologue[]{ // Option 3 responses
                    new ("NED: ... ", QuoteImage.NEDInsultedImage)
                }
            }
        ),

        /* --- Dialogue tree 5 --- */
        new DialogueTree(
            new Monologue("NED: When it comes to flies, you seem pretty weak...", QuoteImage.NEDNormal), // Starting Text
            
            new DialogueOption[3]{ // Options List
                new ("Me? Weak? I was just holding back!", QuoteImage.BARTHOLEMEWProudImage, OptionType.ROAST),
                new ("The only thing I’m weak for is Queen Lily!", QuoteImage.BARTHOLEMEWIdleImage, OptionType.RIZZ),
                new ("Largest paragraph in the game specifically insulting ned", QuoteImage.BARTHOLEMEWIdleImage, OptionType.INSULTNED)
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
}