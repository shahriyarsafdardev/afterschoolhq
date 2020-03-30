$(document).ready(function(){

    loadDefaultGrades();
    return;

	$.get(
        '/services/getgrades/',
        function(data){
            if (data && data.status == 'ok'){
                
                $('.grade-select').html('');

                for (const grade of data.grades){
                    $('.grade-select').append('<option value="' + grade.ID + '">' + grade.Abbreviation + '</option>');
                }
            } else {
                
            }
        }
    );
});

function loadDefaultGrades(){
    const grades = ["Pre-K", "K", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th"];
    $('.grade-select').html('');
    $('.grade-select').append('<option value="">Select...</option>');
    for (const grade of grades){
        $('.grade-select').append('<option value="' + grade + '">' + grade + '</option>');
    }
}