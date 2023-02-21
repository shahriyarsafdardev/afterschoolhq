import React, { useRef } from 'react';
import { StandaloneSearchBox, LoadScript } from '@react-google-maps/api';
import { Grid, Typography, TextField as MuiTextField } from '@material-ui/core';
import cs from 'classnames';
import useStyles from './styles';

function ApiTextField({ formik, question }) {
  const inputRef = useRef();
  const classes = useStyles();
  const { touched, errors, setFieldValue } = formik;
  const { ID, Text, IsRequired: required = false } = question;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  const handlePlaceChanged = () => {
    const [place] = inputRef.current.getPlaces();
    if (place) {
      setFieldValue(ID, place.formatted_address);
    }
  };

  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {Text} &nbsp;
          <Typography className={typeClass}>{typeText}</Typography>
        </Typography>
      </Grid>
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <LoadScript
          googleMapsApiKey="AIzaSyBq47Ro2O69sQhVLEW4_2L0ClOBSW_Srmc"
          libraries={['places']}
        >
          <StandaloneSearchBox
            onLoad={ref => {
              inputRef.current = ref;
            }}
            onPlacesChanged={handlePlaceChanged}
          >
            <MuiTextField
              variant="outlined"
              fullWidth
              {...formik.getFieldProps(question.ID)}
              error={touched[ID] && errors[ID]}
              helperText={touched[ID] && errors[ID]}
              type="text"
              className={cs('form-control', classes.formControlClass)}
              placeholder="Enter your address"
            />
          </StandaloneSearchBox>
        </LoadScript>
      </Grid>
    </Grid>
  );
}

export default ApiTextField;
