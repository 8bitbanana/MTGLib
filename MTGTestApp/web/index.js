
Object.size = function(obj) {
    var size = 0, key;
    for (key in obj) {
        if (obj.hasOwnProperty(key)) size++;
    }
    return size;
};

objects = {}

function tick() {
	loadData().then(function(data) {
		processData(data);
	})
}

function loadData() {
	return $.getJSON("/data")
		.done(function(json) {
			$("#status").text("Connected");
			data = json;
		})
		.fail(function(json) {
			$("#status").text("Disconnected");
		});
}

function getObj(oid) {
	return objects[oid];
}

function generateObj(obj) {
	var s = `${obj.name} : ${obj.typeLine}`;
	if (obj.powerToughness != null) {
		s += ` : ${obj.powerToughness}`;
	}
	var tag = $(`<p>${s}</p>`);
	if (obj.permanentStatus.tapped) {
		tag.addClass("tapped");
	}
	return tag;
}

function fillZone(container, oids) {
	$(container).empty();
	$(container).each(function (index) {
		for (const oid of oids) {
			var obj = getObj(oid);
			$(this).append(generateObj(obj));
		}
	});
}

function getOidsControlledBy(playerid, oids) {
	owned = []
	for (const oid of oids) {
		var obj = getObj(oid);
		if (obj.controller == playerid) {
			owned.push(oid);
		}
	}
	return owned;
}

function setupPlayers(playerCount) {
	$(".main-container .player-container").remove();
	for (var i = 0; i < playerCount; i++) {
		var cont = $(".hidden-container").children().first().clone();
		cont.attr("id", "p" + i);
		$(".main-container").append(cont);
	}
}

function processData(data) {
	setupPlayers(data.players.length);

	objects = data.objects;

	fillZone(".stack-container", data.theStack);

	for (var playerid = 0; playerid < data.players.length; playerid++) {
		var pref = `#p${playerid} `;

		var header = `Player ${playerid+1} - ${data.players[playerid].life} life`
		if (data.players[playerid].manaPool.length > 0) {
			header += " - Mana: ";
			for (const mana of data.players[playerid].manaPool) {
				header += mana;
			}
		}
		$(pref+".player-header h1").text(header);

		var pbattlefield = getOidsControlledBy(playerid, data.battlefield);
		fillZone(pref+".player-battlefield", pbattlefield);
		fillZone(pref+".player-zone-hand", data.players[playerid].hand);
		$(pref+".player-zone-library .zone-count").text(data.players[playerid].library.length);
		$(pref+".player-zone-graveyard .zone-count").text(data.players[playerid].graveyard.length);
		var pexile = getOidsControlledBy(playerid, data.exile);
		$(pref+".player-zone-exile .zone-count").text(pexile.length);
	}
}

window.onload = function() {
	window.setInterval(function() {
		tick();
	}, 100);
}

