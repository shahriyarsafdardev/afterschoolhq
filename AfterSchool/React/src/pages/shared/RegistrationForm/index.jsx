import React from 'react';
import cs from 'classnames';
import { Form, FormikProvider } from 'formik';
import { Box, Button, Grid, Typography, CircularProgress } from '@material-ui/core';

import { Loader, ProgressBar, ShowQuestion } from 'components';
import useStyles from './styles';

export default function RegistrationForm(props) {
  const classes = useStyles();

  const {
    Name,
    isFree,
    formik,
    loading,
    skipAble,
    totalGroups,
    activeGroup,
    lastSection,
    handleSubmit,
    saveDisabled,
    handleSection,
    registrantType,
    saveButtonText,
    ageRestrictions,
    currentQuestions,
    gradeRestrictions,
    handleRedirection,
    genderRestrictions,
    isUpdating = false
  } = props;
  const outerWrapperClass = isUpdating
    ? classes.outerWrapper
    : cs(classes.outerWrapper, classes.outerWrapperMarginTop);
  return (
    <FormikProvider value={formik}>
      <Form autoComplete="off" noValidate onSubmit={handleSubmit} className={classes.form}>
        {loading ? (
          <Loader />
        ) : (
          <Box className={outerWrapperClass}>
            {isUpdating && (
              <Box className={classes.exitButtonWrapper}>
                <Button
                  color="primary"
                  variant="outlined"
                  className={classes.backButton}
                  onClick={() => handleRedirection()}
                  data-testid={`Exit-${Name}`}
                >
                  Exit
                </Button>
              </Box>
            )}
            <ProgressBar activeGroup={activeGroup} totalGroups={totalGroups} />
            <Box className={classes.formWrapper}>
              <Box className={classes.formInnerWrapper}>
                <Typography
                  style={{
                    padding: '30px 0px'
                  }}
                  variant="h6"
                  color="primary"
                >
                  {Name}
                </Typography>
                <Grid container className={cs(classes.justifyContentCenter, classes.leftPadding)}>
                  <Grid className={classes.innerContainer} item xs={12} md={12} lg={12}>
                    {currentQuestions?.map(question =>
                      ShowQuestion({
                        isUpdating,
                        formik: formik,
                        ageRestrictions,
                        gradeRestrictions,
                        genderRestrictions,
                        currentQuestion: question,
                        currentQuestions: currentQuestions
                      })
                    )}
                  </Grid>
                </Grid>
                <Box className={classes.buttonOuterWrapper}>
                  <Box className={classes.buttonWrapper}>
                    <Button
                      color="primary"
                      variant="outlined"
                      className={classes.backButton}
                      disabled={!activeGroup > 0}
                      onClick={() => handleSection('previous')}
                      data-testid={`Back-${Name}`}
                    >
                      Back
                    </Button>
                    <Button
                      data-testid={`Save-${Name}`}
                      color="primary"
                      className={classes.saveButton}
                      variant="contained"
                      type="submit"
                      disabled={saveDisabled}
                      key={activeGroup}
                    >
                      {saveDisabled ? (
                        <>
                          {'Loading . . .'} <CircularProgress size={'20px'} />
                        </>
                      ) : (
                        saveButtonText(lastSection, registrantType, isFree, skipAble)
                      )}
                    </Button>
                  </Box>
                </Box>
              </Box>
            </Box>
          </Box>
        )}
      </Form>
    </FormikProvider>
  );
}
