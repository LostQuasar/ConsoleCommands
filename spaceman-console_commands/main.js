"use strict";

class main {
    constructor() {
        const basePath = "/mod/console_commands/"

        Logger.info(`Loading: spaceman-console_commands`);

        HttpRouter.onStaticRoute[basePath + "pmcConversionChance"] = { "spaceman-console_commands": this.setPmcPercent.bind(this) };
        HttpRouter.onStaticRoute[basePath + "killServer"] = { "spaceman-console_commands": this.killServer.bind(this) };
        HttpRouter.onStaticRoute[basePath + "insuranceReturnChance"] = { "spaceman-console_commands": this.insuranceReturnChance.bind(this) };
        HttpRouter.onStaticRoute[basePath + "bossChance"] = { "spaceman-console_commands": this.bossChance.bind(this) };
        
        HttpRouter.onDynamicRoute[basePath + "give"] = { "spaceman-console_commands": this.giveItem.bind(this) };
    };

    getConfig(url, info, sessionID) {
        return HttpResponse.getBody(configJson);
    }

    setPmcPercent(url, info, sessionID) {
        Logger.info(`Console Commands: Setting pmcBot chance to ${info}%`);
        BotConfig.pmc.types.assault = info;
        BotConfig.pmc.types.cursedassault = info;
        BotConfig.pmc.types.pmcBot = info;
        return HttpResponse.emptyArrayResponse();

    }

    insuranceReturnChance(url, info, sessionID) {
        Logger.info(`Console Commands: Setting insurance return chance to ${info}%`);
        InsuranceConfig.returnChance = info;
        return HttpResponse.emptyArrayResponse();
    }

    killServer(url, info, sessionID) {
        this.queueExit();
        return HttpResponse.emptyArrayResponse();
    }

    giveItem(url, info, sessionID) {
        var urlList = url.split('/')
        const item_id = urlList[4];
        const requestedAmount = urlList[5];

        if (DatabaseServer.tables.templates.items[item_id]) {
            const maxStackAmount = DatabaseServer.tables.templates.items[item_id]._props.StackMaxSize;

            let lastStackAmount = requestedAmount % maxStackAmount;
            let itemAmount = Math.floor((requestedAmount - lastStackAmount) / maxStackAmount);

            var itemsToGive = []
            const parentId = ProfileController.getPmcProfile(sessionID).Inventory.items[0]._id;

            if (lastStackAmount !== 0) {
                itemsToGive.push(
                    {
                        "_id": this.randomId(),
                        "_tpl": item_id,
                        "parentId": parentId,
                        "slotId": "hideout",
                        "upd": {
                            "SpawnedInSession": true,
                            "StackObjectsCount": lastStackAmount
                        }
                    }
                )
            }

            if (itemAmount > 0) {
                for (var i = 0; i < itemAmount; i++) {
                    itemsToGive.push(
                        {
                            "_id": this.randomId(),
                            "_tpl": item_id,
                            "parentId": parentId,
                            "slotId": "hideout",
                            "upd": {
                                "SpawnedInSession": true,
                                "StackObjectsCount": maxStackAmount
                            }
                        }
                    )
                }
            }

            let messageContent = {
                "text": "Here are the items you ordered",
                "type": 13,
                "maxStorageTime": 3600
            }

            DialogueController.addDialogueMessage("579dc571d53a0658a154fbec", messageContent, sessionID, itemsToGive);
        }
        else {
            Logger.error(`Item id ${item_id} does not exist`);
        }

        return HttpResponse.emptyArrayResponse();
    }

    bossChance(url, info, sessionID) {
        Logger.info(`Console Commands: Setting boss chance to ${info}%`);
        for (let i in DatabaseServer.tables.locations) {
            if (i !== "base") {
                if (DatabaseServer.tables.locations[i].base.BossLocationSpawn !== []) {
                    for (let x in DatabaseServer.tables.locations[i].base.BossLocationSpawn) {
                        DatabaseServer.tables.locations[i].base.BossLocationSpawn[x].BossChance = info
                    }
                }
            }
        }
        return HttpResponse.emptyArrayResponse();
    }


    randomId() {
        const amount = 24;
        const validCharacters = "abcdefghijklmnopqrstuvwxyz0123456789"
        var output = "";
        for (var i = 0; i < amount; i++) {
            var newChar = validCharacters[Math.floor(Math.random() * validCharacters.length)];
            output += newChar;
        }
        return output;
    }

    async queueExit() {
        setTimeout(() => { process.exit(1) }, 500);
    }
}

module.exports.main = new main;