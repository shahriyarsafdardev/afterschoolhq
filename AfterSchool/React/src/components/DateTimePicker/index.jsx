import 'date-fns';
import * as Yup from 'yup';
import DateFnsUtils from '@date-io/date-fns';
import TodayIcon from '@material-ui/icons/Today';
import React, { useEffect, useState } from 'react';
import { Typography, Box, FormHelperText } from '@material-ui/core';
import {
  MuiPickersUtilsProvider,
  KeyboardTimePicker,
  KeyboardDatePicker
} from '@material-ui/pickers';

import {
  changeDateTimeFormat,
  convertDateFormat,
  getAndFormatDateTime,
  getTimeFromDate
} from 'utils';
import useStyles from './styles';

export default function DateTimePicker(props) {
  const {
    error,
    value,
    helperText,
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
    });

  useEffect(() => {
    const dateTime = `${selectedDate} ${changeDateTimeFormat(selectedTime).split(', ')[1]}`;
    dateTimeValidation
      .validate(dateTime)
      .then(() => {
        setFieldValue(dateTime);
        setErrors('');
      })
      .catch(err => {
        setErrors(err.message);
      });
  }, [selectedDate, selectedTime]);

  const handleDateChange = date => {
    if (date) setSelectedDate(convertDateFormat(date).split(', ')[0]);
  };

  const handleTimeChange = time => {
    if (time) setSelectedTime(getAndFormatDateTime(new Date(time)));
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
              onAccept={date => {
                handleDateChange(date);
                setOpenDate(false);
                setOpenTime(true);
                //TODO: Move to function
              }}
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
              onAccept={time => {
                handleTimeChange(time);
                setOpenTime(false);
                //TODO: move to function
              }}
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
      {//TODO: Replace with ErrorBand
      (errors || error) && (
        <Box className={classes.flexCenter}>
          <FormHelperText data-testid="date-time-error" className={classes.error}>
            {errors || helperText}
          </FormHelperText>
        </Box>
      )}
    </Box>
  );
}
