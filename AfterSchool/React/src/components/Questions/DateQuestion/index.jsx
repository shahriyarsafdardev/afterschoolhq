import 'date-fns';
import DateFnsUtils from '@date-io/date-fns';
import { Box, FormHelperText } from '@material-ui/core';
import React, { useRef, useState, useEffect } from 'react';
import { MuiPickersUtilsProvider, KeyboardDatePicker } from '@material-ui/pickers';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import useStyles from './styles';

function DateQuestion({ question, formik }) {
  const classes = useStyles();
  const datePicker = useRef();
  const [error, setError] = useState('');
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Text: title, IsRequired: isRequired = false, QuestionDataTypeId } = question;

  const today = new Date();
  const datePickerProps = { maxDate: today };

  const [selectedDate, setSelectedDate] = useState(values[ID] || null);

  useEffect(() => {
    setFieldValue(ID, selectedDate);
  }, [selectedDate]);

  const formatDate = date => {
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    }).format(date);
  };

  const handleDateChange = date => {
    try {
      if (date === null) {
        setFieldValue(ID, '');
        setError(null);
        return;
      }
      const formattedDate = formatDate(date);
      setSelectedDate(formattedDate);
      setFieldValue(ID, formattedDate);
      setError(null);
    } catch (e) {
      setError(e.message);
      setFieldValue(ID, date);
    }
  };

  const handleError = e => {
    setError(e);
  };

  useEffect(() => {
    setError(touched[ID] && errors[ID] ? errors[ID] : error);
  }, [error, touched[ID], errors[ID]]);

  return (
    <>
      <FieldWrapper label={title} isRequired={isRequired}>
        <div className={classes.dateWrapper}>
          <div className={classes.datePicker} ref={datePicker}>
            <MuiPickersUtilsProvider utils={DateFnsUtils}>
              <KeyboardDatePicker
                {...datePickerProps}
                autoFocus
                format="MM/dd/yyyy"
                margin="normal"
                id="date-picker-dialog"
                value={values[ID] || null}
                onError={handleError}
                onChange={handleDateChange}
                helperText=""
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
        {error && (
          <Box className={classes.flexCenter}>
            <FormHelperText className={classes.maxWidth} error>
              {error}
            </FormHelperText>
          </Box>
        )}
      </FieldWrapper>
    </>
  );
}

export default DateQuestion;
