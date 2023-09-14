import 'date-fns';
import * as Yup from 'yup';
import DateFnsUtils from '@date-io/date-fns';
import TodayIcon from '@material-ui/icons/Today';
import React, { useEffect, useState } from 'react';
import { Typography, Box } from '@material-ui/core';
import {
  MuiPickersUtilsProvider,
  KeyboardTimePicker,
  KeyboardDatePicker
} from '@material-ui/pickers';

import useStyles from './styles';
import { ErrorBand } from 'components';
import { changeDateTimeFormat, getAndFormatDateTime, getTimeFromDate } from 'utils';

export default function DateTimePicker(props) {
  const {
    error,
    value,
    helperText,
    setIsValid,
    customLabel,
    setFieldValue,
    required = false,
    allowFutureDate = false
  } = props;
  const [errors, setErrors] = useState('');
  const classes = useStyles(error || Boolean(errors));
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  const [openDate, setOpenDate] = useState(false);
  const [openTime, setOpenTime] = useState(false);
  const [selectedDate, setSelectedDate] = useState(value.split(', ')[0]);
  const [selectedTime, setSelectedTime] = useState(getAndFormatDateTime(new Date(value)));

  const dateTimeValidation = Yup.date()
    .typeError('Invalid Format')
    .test('Future date test', `${customLabel} should not be in future`, function(value) {
      if (allowFutureDate) return true;

      return !(getTimeFromDate(new Date(value)) > getTimeFromDate(new Date()));
    })
    .test('Date before 1900', `${customLabel} should not be before 1900-01-01`, function(value) {
      return !(getTimeFromDate(new Date(value)) < getTimeFromDate(new Date('01/01/1900')));
    });

  const validateDateTime = dateTime =>
    dateTimeValidation
      .validate(dateTime)
      .then(() => {
        setFieldValue(dateTime);
        setErrors('');
        setIsValid(true);
      })
      .catch(err => {
        setErrors(err.message);
        setIsValid(false);
      });

  useEffect(() => {
    const dateTime = `${selectedDate} ${changeDateTimeFormat(selectedTime).split(', ')[1]}`;
    validateDateTime(dateTime);
  }, [selectedDate, selectedTime]);

  const handleDateChange = date => {
    if (date) {
      setSelectedDate(changeDateTimeFormat(date).split(', ')[0]);
    } else {
      setErrors('Invalid Format');
      setIsValid(false);
    }
  };

  const handleTimeChange = time => {
    if (time) {
      setSelectedTime(getAndFormatDateTime(new Date(time)));
    } else {
      setErrors('Invalid Format');
      setIsValid(false);
    }
  };

  const handleDateOnAccept = date => {
    handleDateChange(date);
    setOpenDate(false);
    setOpenTime(true);
  };

  const handleTimeOnAccept = time => {
    handleTimeChange(time);
    setOpenTime(false);
  };

  return (
    <Box>
      {customLabel && (
        <Typography variant="h6">
          {customLabel}
          &nbsp;
          <Typography component="span" className={typeClass}>
            {typeText}
          </Typography>
        </Typography>
      )}

      <Box className={classes.dateWrapper}>
        <Box className={classes.datePicker}>
          <MuiPickersUtilsProvider utils={DateFnsUtils}>
            <KeyboardDatePicker
              margin="normal"
              id="date-picker-dialog"
              data-testid="date-picker"
              format="yyyy-MM-dd"
              value={selectedDate}
              helperText=""
              error={false}
              onChange={date => handleDateChange(date)}
              KeyboardButtonProps={{
                'aria-label': 'change date'
              }}
              onClick={() => setOpenDate(true)}
              open={openDate}
              onAccept={date => handleDateOnAccept(date)}
              onDismiss={() => setOpenDate(false)}
            />
            <KeyboardTimePicker
              margin="normal"
              id="time-picker"
              data-testid="time-picker"
              value={selectedTime}
              helperText=""
              error={false}
              onChange={time => handleTimeChange(time)}
              KeyboardButtonProps={{
                'aria-label': 'change time'
              }}
              open={openTime}
              onClick={() => setOpenTime(true)}
              onAccept={time => handleTimeOnAccept(time)}
              onDismiss={() => setOpenTime(false)}
            />
          </MuiPickersUtilsProvider>
          <Box>
            <TodayIcon
              fontSize="large"
              data-testid="date-time-picker"
              onClick={() => setOpenDate(true)}
            />
          </Box>
        </Box>
      </Box>
      {(errors || error) && <ErrorBand error={errors || helperText} />}
    </Box>
  );
}
