ALTER TABLE `creatures` ADD `strBonus` FLOAT NOT NULL DEFAULT '0' AFTER `luckFood`, ADD `intBonus` FLOAT NOT NULL DEFAULT '0' AFTER `strBonus`, ADD `dexBonus` FLOAT NOT NULL DEFAULT '0' AFTER `intBonus`, ADD `willBonus` FLOAT NOT NULL DEFAULT '0' AFTER `dexBonus`, ADD `luckBonus` FLOAT NOT NULL DEFAULT '0' AFTER `willBonus`;