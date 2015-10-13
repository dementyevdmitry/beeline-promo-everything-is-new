$(document).on('click', ".checkbox", function(e) {
	if($(e.target).closest("a").length) return true;
	
	var $hidden = $(this).find("input");
	var $cont = $(this);//.closest(".checkbox");

	if($hidden.is(":checked")) {
		$cont.removeClass("checked");
		$hidden.prop('checked', false);
	} else {
		$cont.addClass("checked");
		$hidden.prop('checked', true);
	}
	
	return false;
});

$(document).on('click', ".fold-link", function(e) {
	var $link = $(this),
		$foldable = $link.next(".foldable");
	
	$foldable.slideToggle(300);
	$link.toggleClass("open");

	return false;
});

$(document).on("click", ".offer-choice .btn-link", function(e) {
	var $choice = $(this).closest(".offer-choice"),
		$success = $(".success-block");
	
	$choice.hide();
	$success.show();
	setTimeout(function() {
		$success.addClass("show");
	},100);
});

$(document).on('click', ".success-block", function(e) {
	var T = $(this),
		$circle = T.find(".circle");
	
	setTimeout(function() {
		$circle.addClass("show");
	},500);

	return false;
});