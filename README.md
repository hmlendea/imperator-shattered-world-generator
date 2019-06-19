[![Build Status](https://travis-ci.com/hmlendea/imperator-shattered-world-generator.svg?branch=master)](https://travis-ci.com/hmlendea/imperator-shattered-world-generator)

# About

Tool for generating Shattered World mods for Imperator: Rome

# Usage

The generator is used as just another console application.

Open a terminal and run the `ImperatorShatteredWorldGenerator` executable, making sure that all the desired arguments are supplied.

# Configuration

The generator settings are supplied via CLI arguments

| Argument                                                 | Description                                | Optional  | Default|
|----------------------------------------------------------|--------------------------------------------|-----------|--------|
| -d<br>--dir<br>--game<br>--imperator | Game directory path. Example:<br><sup>`~/.local/share/Steam/steamapps/common/ImperatorRome`</sup> | Mandatory | N/A |
| -n<br>--name                                             | Name of the output mod                     | Mandatory | N/A    |
| -s<br>--seed                                             | TNG seed to generate Mod                   | Optional  | Random |
| --capital-population<br>--capital-pops                   | Amount of pops for capital cities          | Optional  | 10     |
| --city-population-min<br>--city-pops-min                 | Minimum amount of pops for regular cities  | Optional  | 4      |
| --city-population-max<br>--city-pops-max                 | Maximum amount of pops for regular cities  | Optional  | 12     |
| --city-civilisation-min<br>--city-civ-min                | Minimum civilisation level for cities      | Optional  | 0      |
| --city-civilisation-max<br>--city-civ-max                | Maximum civilisation level for cities      | Optional  | 20     |
| --city-barbarian-min<br>--city-barb-min                  | Minimum barbarian level for cities         | Optional  | 0      |
| --city-barbarian-max<br>--city-barb-max                  | Maximum barbarian level for cities         | Optional  | 0      |
| --country-centralisation-min<br>--country-cent-min       | Minimum centralisation level for countries | Optional  | 0      |
| --country-centralisation-max<br>--country-cent-max       | Maximum centralisation level for countries | Optional  | 0      |
| --random-countries<br>--new-countries<br>--rng-countries | Amount of random new countries to generate | Optional  | 500    |

**Note**: I would advice against generating too many new countries. The game may (or may not) load fine, but it could crash at any point after the game has started (even many hours later).
**Note**: I was not able to generate more than 1,900 countries without the game crashing at startup

# How it works

The generator will load and randomise the pops, culture and religion of all the habitable cities defined in the vanilla game.
It will then proceed to load the vanilla countries and remove all their regions except their capital. The religion and culture of the capital will be set to the country's culture and religion (which usually matched the vanilla ones for that city)
New countries will be generated randomly, based on randomly chosen vacant cities.

**Note**: The game will throw a lot of event errors if some of the vanilla countries are missing. To be on the safe side, all of the vanilla countries will be kept regardless of generator settings. Fixing this would require rewriting many events, and a lot of debugging effort.

# Plans / Goals / Roadmap

 - Random trade goods
 - Random country names (based on culture. probably based on Markov chains)
 - Option to remove vanilla countries
