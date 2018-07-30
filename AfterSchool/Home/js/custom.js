



if (window.matchMedia("(max-width: 992px)").matches) {
    /* The viewport is less than, or equal to, 700 pixels wide */
} else {
       
$(document).ready(function(){
			
			 // Select and loop the container element of the elements you want to equalise
			 $('.container').each(function(){  
			   
			   // Cache the highest
			   var highestBox = 0;
			   
			   // Select and loop the elements you want to equalise
			   $('.match-height', this).each(function(){
			     
			     // If this box is higher than the cached highest then store it
			     if($(this).height() > highestBox) {
			       highestBox = $(this).height(); 
			     }
			   
			   });  
			         
			   // Set the height of all those children to whichever was highest 
			   $('.match-height',this).height(highestBox);
			                 
			 }); 
			
			});


$(".toggle-password").click(function() {

  $(this).toggleClass("fa-eye fa-eye-slash");
  var input = $($(this).attr("toggle"));	
  if (input.attr("type") == "password") {
    input.attr("type", "text");
  } else {
    input.attr("type", "password");
  }
});



}

