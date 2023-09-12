// Register with Angular
angular.module("uMarketingSuite", ["rzSlider", "as.sortable"]);

// Register as dependency for Umbraco
angular.module("umbraco").requires.push("uMarketingSuite");
