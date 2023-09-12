angular.module("uMarketingSuite").component("umsAnalyticsContentApp",{template:'<div tab-focus-once="$ctrl.load()"><div class="u-marketing-suite-dashboard in-content"><ums-analytics ng-if="$ctrl.loaded" content-id="$ctrl.contentId" culture="$ctrl.culture"></ums-analytics></div></div>',controller:["umsUtilService","$element","$scope",function umsAnalyticsContentAppController(utils,$element,$scope){var $ctrl=this;this.bottomBar=null,this.editorContainer=null,this.load=function(){if(this.contentId=utils.getCurrentContentId(),this.culture=utils.getCurrentCulture(),this.bottomBar=document.querySelector(".umb-editor-footer"),this.editorContainer=document.querySelector(".umb-editor-container"),null!=this.bottomBar&&null!=this.editorContainer){var unsubscribe=utils.contentAppOnActivate($element,function(isActive){isActive?$ctrl.hideBottomBar():$ctrl.restoreBottomBar()},!0);$scope.$on("$destroy",unsubscribe),$scope.$on("$destroy",this.restoreBottomBar.bind(this))}this.loaded=!0},this.hideBottomBar=function hideBottomBar(){$ctrl.bottomBar.style.display="none",$ctrl.editorContainer.style.bottom=0},this.restoreBottomBar=function restoreBottomBar(){$ctrl.bottomBar.style.removeProperty("display"),$ctrl.editorContainer.style.removeProperty("bottom")}}]}),angular.module("uMarketingSuite").component("umsPersonalizationContentApp",{template:'<div tab-focus-once="$ctrl.load()"><div class="u-marketing-suite-dashboard in-content"><ums-screens name="Personalization" icon-name="personalization"><ums-screen name="Personalized variants" ng-if="$ctrl.showApplyPersonalization"><ums-applied-personalization-single-page></ums-applied-personalization-single-page></ums-screen><ums-screen name="Content scoring" ng-if="$ctrl.showScorePersonalization"><ums-personalization-scoring ng-if="$ctrl.loaded"></ums-personalization-scoring></ums-screen><ums-screen name="Segment insights" ng-if="$ctrl.showApplyPersonalization"><ums-personalization-segment-insights></ums-personalization-segment-insights></ums-screen><ums-screen name="Applied segments insights" ng-if="$ctrl.showApplyPersonalization"><ums-personalization-applied-segments-insights></ums-personalization-applied-segments-insights></ums-screen></ums-screens></div></div>',controller:["umsUtilService","umsDocumentTypePermissionsService",function umsPersonalizationContentAppController(utils,doctypePermissionService){var $ctrl=this;this.load=function(){this.contentTypeId=utils.getCurrentContentTypeId(),this.showApplyPersonalization=!1,this.showScorePersonalization=!1,doctypePermissionService.getPermission(this.contentTypeId).then(function(permission){$ctrl.showApplyPersonalization=permission.allowApplyPersonalization,$ctrl.showScorePersonalization=permission.allowScorePersonalization}).finally(function(){$ctrl.loaded=!0})}}]}),angular.module("uMarketingSuite").controller("umsPersonalizationContentAppCtrl",["umsUtilService","umsDocumentTypePermissionsService","editorState","umsAppliedPersonalizationService",function umsPersonalizationContentAppCtrl(utils,doctypePermissionService,editorState,service){var $ctrl=this;this.$onInit=function(){!function updateBadgeCount(){var es=editorState.current,appIdx=_.findIndex(es.apps,function(app){return"umsPersonalization"===app.alias});if(appIdx>-1){var badgeElem=document.querySelector("ul.umb-sub-views-nav > li:nth-child("+(appIdx+1)+') umb-editor-navigation-item [ng-show="vm.item.badge"]');if(null!=badgeElem){var nodeId=utils.getCurrentContentId(),contentTypeId=utils.getCurrentContentTypeId(),culture=utils.getCurrentCulture();service.getAllForPage(nodeId,contentTypeId,culture).then(function(items){var count=items.length;0===count?badgeElem.classList.add("ng-hide"):(badgeElem.classList.remove("ng-hide"),badgeElem.innerHTML=count)})}}}()},this.load=function(){this.contentTypeId=utils.getCurrentContentTypeId(),this.showApplyPersonalization=!1,this.showScorePersonalization=!1,doctypePermissionService.getPermission(this.contentTypeId).then(function(permission){$ctrl.showApplyPersonalization=permission.allowApplyPersonalization,$ctrl.showScorePersonalization=permission.allowScorePersonalization}).finally(function(){$ctrl.loaded=!0})}}]),angular.module("uMarketingSuite").component("umsProfilesContentApp",{template:'<div tab-focus-once="$ctrl.load()"><div class="u-marketing-suite-dashboard in-content"><ums-screens name="Profiles" icon-name="profiles"><ums-screen name="Activity"><ums-profile-activity ng-if="!$ctrl.state.isLoading" show-related-profiles="true" visitor-ids="$ctrl.state.visitorIds"></ums-profile-activity></ums-screen></ums-screens></div></div>',controller:["umsProfileInfoService","$location","memberResource",function umsProfilesContentAppController(service,$location,memberResource){var ctrl=this;ctrl.state={isLoading:!0,visitorIds:[]},ctrl.fn={getMemberFromUrl:function(){var url=$location.path();return(url=url.split("/"))[url.length-1]},getMemberIdFromGuid:function(guid){return memberResource.getByKey(guid).then(function(result){return result.id})}},this.load=function(){var memberGuid=ctrl.fn.getMemberFromUrl();0===memberGuid.indexOf("Member")?ctrl.state.isLoading=!1:ctrl.fn.getMemberIdFromGuid(memberGuid).then(function(memberId){service.getRelatedProfiles(memberId).then(function(visitorIds){visitorIds.length>0&&(ctrl.state.visitorIds=visitorIds),ctrl.state.isLoading=!1})})}}]}),angular.module("uMarketingSuite").component("umsAbTestingContentApp",{require:{variantsCtrl:"^umbVariantContentEditors"},template:'<div tab-focus-once="$ctrl.fn.load()"><div class="u-marketing-suite-dashboard in-content" ng-if="$ctrl.enabled" ng-switch="$ctrl.state.view"><div class="ums-topbar__container"><div class="ums-content ums-topbar__content"><div class="ums-topbar__icon"><ums-icon id="icon-ab-testing" modifier="lg"></ums-icon></div><div class="ums-topbar__title">A/B Testing</div><ums-package-status></ums-package-status><div class="ums-topbar__logo"><img src="/App_Plugins/uMarketingSuite/assets/ums-logo.svg" alt></div></div></div><div class="ums-subnav__container"><div class="ums-content"><ul class="ums-subnav__list"></ul></div><div class="ums-contentbar__badge"><ums-icon id="icon-ab-testing" modifier="md"></ums-icon><span>A/B Testing</span></div></div><ums-loader ng-if="$ctrl.state.loaded === false"></ums-loader><div ng-switch-when="main" ng-if="$ctrl.state.loaded"><div class="ums-content umsPaddingTop"><div class="ums-flexgrid__container ums-flexgrid__container--lg is-animated" ng-if="$ctrl.state.testsForPage.length === 0"><div class="ums-flexgrid__item ums-flexgrid__item--1-1 umsPaddingBottom"><div class="ums-testdetail__empty__container"><img src="/App_Plugins/uMarketingSuite/assets/a-b-testing.svg" alt><br><h2>There are no A/B tests created for this page yet</h2><span ng-show="$ctrl.state.contentId === 0">This Umbraco node needs to be saved before you can start a new A/B test</span> <button class="ums-button ums-button--success ums-button--icon" type="button" ng-show="$ctrl.state.contentId > 0" ng-click="$ctrl.fn.startTest()"><span>Start a test</span><ums-icon id="icon-arrow" modifier="btn"></ums-icon></button></div></div></div><div class="ums-flexgrid__container ums-flexgrid__container--lg is-animated" ng-if="$ctrl.state.testsForPage.length > 0"><div class="ums-flexgrid__item ums-flexgrid__item--1-3"><div class="ums-boxed__container is-animated"><h2>A/B testing on this page</h2><div class="umsDescription"><p><span ng-show="$ctrl.state.segmentSupport">Start an A/B test for this page. This will default to a Single Page test which allows you to A/B test Umbraco content.</span> <span ng-show="!$ctrl.state.segmentSupport">Start an A/B test.</span></p></div><br><button class="ums-button ums-button--success ums-button--icon" type="button" ng-click="$ctrl.fn.startTest()"><span>Start a test</span><ums-icon id="icon-arrow" modifier="btn"></ums-icon></button></div></div><div class="ums-flexgrid__item ums-flexgrid__item--2-3 umsPaddingBottom"><ums-ab-testing-test-filters filters="$ctrl.state.filters"></ums-ab-testing-test-filters><div class="ums-alert__container ums-alert__container--warning" ng-if="($ctrl.state.testsForPage | filter:$ctrl.fn.filterTests).length === 0">No tests could be found matching your search criteria.</div><ums-ab-testing-test-list tests="$ctrl.state.testsForPage | filter:{status:2} | filter:$ctrl.fn.filterTests" header="Running" on-click="$ctrl.fn.openTest(test.abTestId, test.status)"></ums-ab-testing-test-list><ums-ab-testing-test-list tests="$ctrl.state.testsForPage | filter:{status:3} | filter:$ctrl.fn.filterTests" header="Stopped" on-click="$ctrl.fn.openTest(test.abTestId, test.status)"></ums-ab-testing-test-list><ums-ab-testing-test-list tests="$ctrl.state.testsForPage | filter:$ctrl.fn.filterScheduledOrDraft | filter:$ctrl.fn.filterTests" header="New" on-click="$ctrl.fn.openTest(test.abTestId, test.status)"></ums-ab-testing-test-list><ums-ab-testing-test-list tests="$ctrl.state.testsForPage | filter:{status:4} | filter:$ctrl.fn.filterTests" header="Completed" on-click="$ctrl.fn.openTest(test.abTestId, test.status)"></ums-ab-testing-test-list></div></div></div></div><div ng-switch-when="create"><ums-ab-testing-create-test edit-variant="$ctrl.fn.editVariant(test, variant, defaultFn)" enable-single-page="$ctrl.state.segmentSupport" test-type="$ctrl.state.segmentSupport ? 0 : 1" back="$ctrl.state.view = \'main\'; $ctrl.fn.refreshTestsForPage()" on-created="$ctrl.fn.onCreatedTest(test)"></ums-ab-testing-create-test></div><div ng-switch-when="detail"><ums-ab-testing-create-test ng-if="$ctrl.state.testDetail.draft" edit-variant="$ctrl.fn.editVariant(test, variant, defaultFn)" on-created="$ctrl.fn.onCreatedTest(test)" back="$ctrl.state.view = \'main\'; $ctrl.fn.refreshTestsForPage()" id="$ctrl.state.testDetail.id" enable-single-page="true"></ums-ab-testing-create-test><ums-ab-testing-test-detail ng-if="!$ctrl.state.testDetail.draft" back="$ctrl.state.view = \'main\'; $ctrl.fn.refreshTestsForPage()" test-id="$ctrl.state.testDetail.id"></ums-ab-testing-test-detail></div></div></div>',controller:["umsUtilService","umsAbTestingService","umsAbTestingCreateTestState","umsAbTestingCreateTestService","umsEventEmitter","umsState","$element","$q","$scope","editorState",function umsAbTestContentAppController(utils,service,createTestState,createTestService,eventEmitter,umsState,$element,$q,$scope,editorState){var ctrl=this,destroyFns=[],state=ctrl.state={contentId:null,culture:null,loaded:null,view:"main",createState:createTestState,testsForPage:[],testDetail:{id:null,draft:!1},segmentSupport:umsState.segmentSupport,filters:{}};this.$onInit=function(){null!=createTestState.test&&(state.view="create"),destroyFns.push(eventEmitter.on("ab-testing.content-app.show-test",function(args){null!=args.id&&args.id!==state.testDetail.id&&fn.loadTest(args.id).then(function(test){null!=test&&fn.openTest(test.id,test.status)})})),destroyFns.push(utils.contentAppOnActivate($element,function(isActive){ctrl.enabled=isActive},!0));var caModel=$scope.$parent.$parent.model;if(null!=caModel&&null!=caModel.viewModel){var es=editorState.current,appIdx=_.findIndex(es.apps,function(app){return app.alias===caModel.alias});if(appIdx>-1){var appTab=document.querySelector("ul.umb-sub-views-nav > li:nth-child("+(appIdx+1)+") umb-editor-navigation-item > button");if(null!=appTab){var badgeElem=document.createElement("div");caModel.viewModel.abTestRunning?(badgeElem.classList.add("badge","ums-ca-badge__item"),badgeElem.innerHTML='<svg class="ums-ca-badge__icon" data-ums-icon-id="ums-badge-icon-play" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 9.01 10">                                <path d="M9.739.22c-.9-.517-1.632-.093-1.632.945V8.833c0,1.04.731,1.463,1.632.946l6.7-3.843a.986.986,0,0,0,0-1.873Z" transform="translate(-8.107)"/>                                </svg>'):caModel.viewModel.abTestScheduled&&(badgeElem.classList.add("badge","ums-ca-badge__item","s-scheduled"),badgeElem.innerHTML='<svg class="ums-ca-badge__icon" data-ums-icon-id="ums-badge-icon-clock" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 559.98 559.98">                                    <path d="M279.99,0C125.601,0,0,125.601,0,279.99c0,154.39,125.601,279.99,279.99,279.99c154.39,0,279.99-125.601,279.99-279.99C559.98,125.601,434.38,0,279.99,0z M279.99,498.78c-120.644,0-218.79-98.146-218.79-218.79c0-120.638,98.146-218.79,218.79-218.79s218.79,98.152,218.79,218.79C498.78,400.634,400.634,498.78,279.99,498.78z"/>                                    <path d="M304.226,280.326V162.976c0-13.103-10.618-23.721-23.716-23.721c-13.102,0-23.721,10.618-23.721,23.721v124.928c0,0.373,0.092,0.723,0.11,1.096c-0.312,6.45,1.91,12.999,6.836,17.926l88.343,88.336c9.266,9.266,24.284,9.266,33.543,0c9.26-9.266,9.266-24.284,0-33.544L304.226,280.326z"/>                                </svg>'),appTab.appendChild(badgeElem)}}}},this.$onDestroy=function(){destroyFns.forEach(function(fn){fn()})};var fn=ctrl.fn={load:function(){state.loaded=!1,state.contentId=utils.getCurrentContentId(),state.culture=utils.getCurrentCulture(),fn.refreshTestsForPage().then(function(){state.loaded=!0})},refreshTestsForPage:function(){return 0===state.contentId?(state.testsForPage=[],$q.when()):service.getAllForPage(state.contentId).then(function(response){state.testsForPage=response.data})},loadTest:function(id){return service.getTestDetails(id).then(function(response){return response.data})},openTest:function(id,status){state.testDetail.id=id,state.testDetail.draft=5===status,state.view="detail"},onCreatedTest:function(test){fn.openTest(test.id,test.status),fn.refreshTestsForPage()},clearTestCreateState:function(){eventEmitter.emit("ab-testing.clear-create-state")},startTest:function(){fn.clearTestCreateState(),state.view="create"},editVariant:function(test,variant,defaultFn){function next(variant){_.any(ctrl.variantsCtrl.content.variants,function(v){return(null==v.culture||v.culture===state.culture)&&v.segment==variant.segment})?ctrl.fn.openInSplitview(state.culture,variant.segment):service.createSegment(state.contentId,state.culture,variant.segment).then(function(){utils.reloadNode(function(){setTimeout(function(){ctrl.fn.openInSplitview(state.culture,variant.segment)})})})}0===test.testType?service.saveTest(test,!1).then(function(response){var testId=response.data.id;if(null!=response.data.test&&(createTestState.test=response.data.test),createTestState.test.id=testId,null==variant.id||0===variant.id){var variantIndex=test.variants.indexOf(variant),newVariant=createTestState.test.variants[variantIndex];null!=newVariant&&newVariant.id>0?next(newVariant):service.getTestDetails(testId).then(function(response){next(response.data.variants[variantIndex])})}else next(variant)}):defaultFn()},openInSplitview:function(culture,segment){eventEmitter.emit("open-splitview",{culture:culture,segment:segment})},previewVariant:createTestService.fn.previewVariant,filterTests:function(test){return test=test||{},!(!(state.filters.waiting||1!==test.status&&5!==test.status)||!state.filters.running&&2===test.status||!(state.filters.completed||3!==test.status&&4!==test.status)||!(state.filters.reliable&&test.reliableVariants>0||state.filters.unreliable&&0===test.reliableVariants)||state.filters.userId&&test.createdByUmbracoUserId!=state.filters.userId)},filterScheduledOrDraft:function(test){return test&&(1===test.status||5===test.status)}}}]});