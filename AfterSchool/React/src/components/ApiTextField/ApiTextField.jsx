import cs from 'classnames';
import useStyles from './styles';
import React, { useRef, useState, useEffect } from 'react';
import { StandaloneSearchBox, LoadScript } from '@react-google-maps/api';
import { Typography, TextField as MuiTextField, Box } from '@material-ui/core';
import { locationObject } from 'components/SchoolField/SchoolField';

function ApiTextField({ formik, question, currentQuestions }) {
  const inputRef = useRef();
  const classes = useStyles();
  const [disabled, setDisable] = useState(true);
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Text, IsRequired: required = false, QuestionDataTypeId } = question;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  const setAddressFields = place => {
    currentQuestions.map(question => {
      const type = question.Text;
      switch (type) {
        case 'City':
          setFieldValue(question.ID, place.city || '');
          break;
        case 'Zip Code':
          setFieldValue(question.ID, place.postalcode || '');
          break;
        case 'State':
          if (place.state) {
            const options = question.Options.split(',').map(option => ({
              key: option,
              value: option
            }));
            const selectedOption = options.find(option => option.value === place.state);
            if (selectedOption) {
              setFieldValue(question.ID, JSON.stringify([selectedOption]));
            }
          }
          break;
      }
    });
  };

  const handlePlaceChanged = () => {
    const [place] = inputRef.current.getPlaces();
    if (place) setFieldValue(ID, place.name);
    setAddressFields(locationObject(place));
  };

  useEffect(() => {
    if (!values[11111] && disabled) return setDisable(false);
    setDisable(true);
    setFieldValue(ID, '');
  }, [values[11111]]);

  return (
    <Box className={classes.outerWrapper}>
      <Box className={classes.fieldLabelWrapper}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {Text} &nbsp;
          <Typography component="span" className={typeClass}>
            {typeText}
          </Typography>
        </Typography>
      </Box>

      <Box className={classes.textFieldOuterWrapper}>
        <Box className={classes.textFieldContainer}>
          <LoadScript googleMapsApiKey={process.env.GOOGLE_MAPS_API_KEY} libraries={['places']}>
            <StandaloneSearchBox
              className={classes.searchBox}
              onLoad={ref => {
                inputRef.current = ref;
              }}
              onPlacesChanged={handlePlaceChanged}
            >
              <MuiTextField
                inputProps={{
                  'data-testid': `${Text}-${QuestionDataTypeId}`
                }}
                variant="outlined"
                fullWidth
                {...formik.getFieldProps(question.ID)}
                error={touched[ID] && errors[ID]}
                helperText={touched[ID] && errors[ID]}
                type="text"
                className={cs('form-control', classes.formControlClass)}
                placeholder={`Enter your ${Text}`}
                disabled={disabled}
              />
            </StandaloneSearchBox>
          </LoadScript>
        </Box>
      </Box>
    </Box>
  );
}

export default ApiTextField;
