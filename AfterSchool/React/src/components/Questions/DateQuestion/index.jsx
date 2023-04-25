import React, { useRef } from 'react';
import DayjsUtils from '@date-io/dayjs';
import { Box, FormHelperText } from '@material-ui/core';
import { MuiPickersUtilsProvider, KeyboardDatePicker } from '@material-ui/pickers';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import { restrictedQuestionsDataTypeIds } from 'constants';
import useStyles from './styles';

function DateQuestion({
  ageRestrictions: { isAgeRestricted = false, minAge, maxAge },
  question,
  formik
}) {
  const classes = useStyles();
  const datePicker = useRef();
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Text: title, IsRequired: isRequired = false, QuestionDataTypeId } = question;

  const handleDateChange = date => {
    setFieldValue(
      ID,
      new Intl.DateTimeFormat('en-US', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      }).format(date)
    );
  };

  const handleError = () => {};
  const today = new Date();
  const datePickerProps = { maxDate: today };
  if (isAgeRestricted && QuestionDataTypeId === restrictedQuestionsDataTypeIds.dateOfBirth) {
    const startingDate = noOfYears =>
      new Date(today.getFullYear() - noOfYears, today.getMonth(), today.getDate());

    datePickerProps['maxDate'] = startingDate(minAge);
    datePickerProps['minDate'] = startingDate(maxAge);
  }

  return (
    <>
      <FieldWrapper label={title} isRequired={isRequired}>
        <div className={classes.dateWrapper}>
          <div className={classes.datePicker} ref={datePicker}>
            <MuiPickersUtilsProvider utils={DayjsUtils}>
              <KeyboardDatePicker
                {...datePickerProps}
                InputProps={{ readOnly: true }}
                autoFocus
                variant="inline"
                format="MM/DD/YYYY"
                margin="normal"
                id="date-picker-inline"
                value={values[ID] || null}
                onError={handleError}
                onChange={handleDateChange}
                autoOk
                classes={{
                  root: classes.dateInput
                }}
                PopoverProps={{
                  anchorEl: datePicker.current,
                  anchorOrigin: {
                    horizontal: 150,
                    vertical: 'bottom'
                  }
                }}
                KeyboardButtonProps={{
                  'aria-label': 'change date',
                  'data-testid': `${title}-${QuestionDataTypeId}`
                }}
              />
            </MuiPickersUtilsProvider>
          </div>
        </div>
        {touched[ID] && errors[ID] && (
          <Box className={classes.flexCenter}>
            <FormHelperText className={classes.maxWidth} error>
              {errors[ID]}
            </FormHelperText>
          </Box>
        )}
      </FieldWrapper>
    </>
  );
}

export default DateQuestion;
